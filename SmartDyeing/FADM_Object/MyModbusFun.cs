using Lib_Card;
using Lib_Card.ADT8940A1;
using Lib_Card.ADT8940A1.Module;
using Lib_Card.ADT8940A1.Module.Home;
using Lib_Card.ADT8940A1.Module.Put;
using SmartDyeing.FADM_Auto;
using SmartDyeing.FADM_Control;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.AxHost;

namespace SmartDyeing.FADM_Object
{
    class MyModbusFun
    {

        //nNeedCheck是否需要处理未发现针筒
        //i_type 0为不发停止(复位时不响应停止)，1为发停止
        public static int GetReturn(int i_type)
        {

            int[] ia_array2 = { 0, 0, 0, 0, 0 };
            List<int> lis_data = new List<int>();
            List<string> lis_array = new List<string>();
            bool b_send = false;

            Communal._b_isUpdateNewData = false;

            while (true) //这里一直读取。然后天平那边分开读取 也会造成数据混乱问题 所以这里直接读4个字节。天平、光幕和执行完成
            {
                if (i_type == 1)
                {
                    if (FADM_Object.Communal._b_stop)
                    {
                        if (!b_send)
                        {
                            //输出停止信号
                            int[] ia_array = { 1 };
                            int i_state = FADM_Object.Communal._tcpModBus.Write(839, ia_array);
                            if (i_state != -1)
                            {
                                //FADM_Object.Communal._b_stop = false;
                                //return -2;
                                b_send = true;
                            }
                        }
                    }
                }
                //if (Dye._cup_Temps[i_cupNo - 1]._i_lockUp == 1)
                if (!Communal._b_isBSendOpen)
                {
                    if (Communal._i_bType == 1)
                    {
                        int[] ia_array = { 17 };
                        int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        Communal._b_isBSendClose = false;
                        Communal._b_isBSendOpen = true;
                    }
                }

                if (!Communal._b_isBSendClose)
                {
                    if (Communal._i_bType == 2)
                    {
                        int[] ia_array = { 18 };
                        int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        Communal._b_isBSendOpen = false;
                        Communal._b_isBSendClose = true;
                    }
                }
                //一直读d904 执行完成标志位
                ia_array2 = AwaitSuccess();
                if (ia_array2[4] == 2 || ia_array2[4] == 3)
                {
                    Lib_Log.Log.writeLogException("读取标志位退出" + ia_array2[4].ToString());
                    Console.WriteLine("读取标志位退出");
                    //ClearSuccessState();//清除标志位
                    break;
                }
                else
                {
                    //判断是否有错误码
                    if (ia_array2[0] == 1)
                    {
                        int[] ia_errArray = new int[100];
                        int i_state = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state != -1)
                        {
                            //插入报警信息
                            //先判断是否有结束错误，有就不显示询问项
                            for (int i = 0; i < ia_errArray.Length; i++)
                            {
                                //只有询问才显示在这里，异常统一在904为3时一起显示
                                if (ia_errArray[i] > 10000)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Keys.Contains(ia_errArray[i]))
                                    {

                                        //如果播报没存在，就加入
                                        if (!lis_data.Contains(i))
                                        {
                                            lis_data.Add(i);
                                            //循环判断是否已加入报警
                                            string s_message = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ia_errArray[i]];
                                            string s_insert = CardObject.InsertD(s_message, " GetReturn");
                                            //将对应报警记录下来，方便选择交互后重新写入对应位置
                                            Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                            prompt = Lib_Card.CardObject.keyValuePairs[s_insert];
                                            prompt.Count = i;
                                            Lib_Card.CardObject.keyValuePairs[s_insert] = prompt;

                                            lis_array.Add(s_insert);
                                        }
                                    }
                                    else
                                    {
                                        Lib_Log.Log.writeLogException("异常返回");
                                    }
                                }

                            }
                            for (int i = lis_array.Count - 1; i >= 0; i--)
                            {
                                int i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Choose;
                                int i_count = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Count;
                                string s_info = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Info;
                                if (i_alarm_Choose != 0)
                                {
                                    string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','GetReturn','" +
                                         s_info + "(" + (i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    CardObject.DeleteD(lis_array[i]);
                                    lis_array.Remove(lis_array[i]);
                                    lis_data.Remove(i_count);
                                    if (i_alarm_Choose == 1)
                                    {
                                        Communal._b_isWriting = true;
                                    //写814地址
                                    lab814:
                                        int[] ia_array4 = { 1 };
                                        i_state = FADM_Object.Communal._tcpModBus.Write(3100 + i_count, ia_array4);
                                        if (i_state == -1)
                                            goto lab814;
                                        //清空903地址
                                        ia_array4[0] = 0;

                                        Thread.Sleep(200);
                                        Communal._b_isWriting = false;
                                    }
                                    else
                                    {
                                        Communal._b_isWriting = true;
                                    lab814:
                                        int[] ia_array4 = { 2 };
                                        i_state = FADM_Object.Communal._tcpModBus.Write(3100 + i_count, ia_array4);
                                        if (i_state == -1)
                                            goto lab814;
                                        //throw new Exception("-2");

                                        Thread.Sleep(200);
                                        Communal._b_isWriting = false;

                                    }
                                }
                            }

                        }
                    }
                }
                Console.WriteLine("tcp读取执行完成标志位" + ia_array2[4].ToString());
                if (ia_array2[4].ToString().Equals("0"))
                {
                    // Console.WriteLine(123);
                }
                Thread.Sleep(1);
            }
            if (ia_array2[4] == 2)
            {
                Lib_Log.Log.writeLogException("执行完成");
                if (FADM_Object.Communal._b_stop)
                {
                    FADM_Object.Communal._b_stop = false;
                    return -2;
                }
                Console.WriteLine("执行完成");
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
            }
            else if (ia_array2[4] == 3)
            {
                Thread.Sleep(200);
                Lib_Log.Log.writeLogException("执行异常了,准备读取错误信息地址");
                Console.WriteLine("执行异常了,准备读取错误信息地址");

                throw new Exception("-2");//跳到外面异常 自己读取异常地址
            }
            return 0;
        }

        //i_type 0为不发停止(复位时不响应停止)，1为发停止
        //b_lock是否锁止
        public static int GetReturn(int i_type ,bool b_lock)
        {

            int[] ia_array2 = { 0, 0, 0, 0, 0 };
            List<int> lis_data = new List<int>();
            List<string> lis_array = new List<string>();
            bool b_send = false;

            Communal._b_isUpdateNewData = false;

            while (true) //这里一直读取。然后天平那边分开读取 也会造成数据混乱问题 所以这里直接读4个字节。天平、光幕和执行完成
            {
                if (i_type == 1)
                {
                    if (FADM_Object.Communal._b_stop)
                    {
                        if (!b_send)
                        {
                            //输出停止信号
                            int[] ia_array = { 1 };
                            int i_state = FADM_Object.Communal._tcpModBus.Write(839, ia_array);
                            if (i_state != -1)
                            {
                                //FADM_Object.Communal._b_stop = false;
                                //return -2;
                                b_send = true;
                            }
                        }
                    }
                }
                //if (Dye._cup_Temps[i_cupNo - 1]._i_lockUp == 1)
                if (!Communal._b_isBSendOpen)
                {
                    if (Communal._i_bType == 1)
                    {
                        int[] ia_array = { 17 };
                        int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        Communal._b_isBSendClose = false;
                        Communal._b_isBSendOpen = true;
                    }
                }

                if (!Communal._b_isBSendClose)
                {
                    if (Communal._i_bType == 2)
                    {
                        int[] ia_array = { 18 };
                        int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array);

                        Communal._b_isBSendOpen = false;
                        Communal._b_isBSendClose = true;
                    }
                }
                //一直读d904 执行完成标志位
                ia_array2 = AwaitSuccess(true);
                if (ia_array2[4] == 2 || ia_array2[4] == 3)
                {
                    Lib_Log.Log.writeLogException("读取标志位退出" + ia_array2[4].ToString());
                    Console.WriteLine("读取标志位退出");
                    //ClearSuccessState();//清除标志位
                    break;
                }
                else
                {
                    //判断是否有错误码
                    if (ia_array2[0] == 1)
                    {
                        int[] ia_errArray = new int[100];
                        int i_state = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state != -1)
                        {
                            //插入报警信息
                            //先判断是否有结束错误，有就不显示询问项
                            for (int i = 0; i < ia_errArray.Length; i++)
                            {
                                //只有询问才显示在这里，异常统一在904为3时一起显示
                                if (ia_errArray[i] > 10000)
                                {
                                    if (SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew.Keys.Contains(ia_errArray[i]))
                                    {

                                        //如果播报没存在，就加入
                                        if (!lis_data.Contains(i))
                                        {
                                            lis_data.Add(i);
                                            //循环判断是否已加入报警
                                            string s_message = SmartDyeing.FADM_Object.Communal._dic_errModbusNoNew[ia_errArray[i]];
                                            string s_insert = CardObject.InsertD(s_message, " GetReturn");
                                            //将对应报警记录下来，方便选择交互后重新写入对应位置
                                            Lib_Card.CardObject.prompt prompt = new Lib_Card.CardObject.prompt();
                                            prompt = Lib_Card.CardObject.keyValuePairs[s_insert];
                                            prompt.Count = i;
                                            Lib_Card.CardObject.keyValuePairs[s_insert] = prompt;

                                            lis_array.Add(s_insert);
                                        }
                                    }
                                    else
                                    {
                                        Lib_Log.Log.writeLogException("异常返回");
                                    }
                                }

                            }
                            for (int i = lis_array.Count - 1; i >= 0; i--)
                            {
                                int i_alarm_Choose = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Choose;
                                int i_count = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Count;
                                string s_info = Lib_Card.CardObject.keyValuePairs[lis_array[i]].Info;
                                if (i_alarm_Choose != 0)
                                {
                                    string s_sql = "INSERT INTO alarm_table" +
                                         "(MyDate,MyTime,AlarmHead,AlarmDetails)" +
                                         " VALUES( '" +
                                         String.Format("{0:d}", DateTime.Now) + "','" +
                                         String.Format("{0:T}", DateTime.Now) + "','GetReturn','" +
                                         s_info + "(" + (i_alarm_Choose == 1 ? "Yes" : "No") + ")');";

                                    FADM_Object.Communal._fadmSqlserver.ReviseData(s_sql);

                                    CardObject.DeleteD(lis_array[i]);
                                    lis_array.Remove(lis_array[i]);
                                    lis_data.Remove(i_count);
                                    if (i_alarm_Choose == 1)
                                    {
                                        Communal._b_isWriting = true;
                                    //写814地址
                                    lab814:
                                        int[] ia_array4 = { 1 };
                                        i_state = FADM_Object.Communal._tcpModBus.Write(3100 + i_count, ia_array4);
                                        if (i_state == -1)
                                            goto lab814;
                                        //清空903地址
                                        ia_array4[0] = 0;

                                        Thread.Sleep(200);
                                        Communal._b_isWriting = false;
                                    }
                                    else
                                    {
                                        Communal._b_isWriting = true;
                                    lab814:
                                        int[] ia_array4 = { 2 };
                                        i_state = FADM_Object.Communal._tcpModBus.Write(3100 + i_count, ia_array4);
                                        if (i_state == -1)
                                            goto lab814;
                                        //throw new Exception("-2");

                                        Thread.Sleep(200);
                                        Communal._b_isWriting = false;

                                    }
                                }
                            }

                        }
                    }
                }
                Console.WriteLine("tcp读取执行完成标志位" + ia_array2[4].ToString());
                if (ia_array2[4].ToString().Equals("0"))
                {
                    // Console.WriteLine(123);
                }
                Thread.Sleep(1);
            }
            if (ia_array2[4] == 2)
            {
                Lib_Log.Log.writeLogException("执行完成");
                if (FADM_Object.Communal._b_stop)
                {
                    FADM_Object.Communal._b_stop = false;
                    return -2;
                }
                Console.WriteLine("执行完成");
                //FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "回零完成");
            }
            else if (ia_array2[4] == 3)
            {
                Thread.Sleep(200);
                Lib_Log.Log.writeLogException("执行异常了,准备读取错误信息地址");
                Console.WriteLine("执行异常了,准备读取错误信息地址");

                throw new Exception("-2");//跳到外面异常 自己读取异常地址
            }
            return 0;
        }

        public static void OpenOrCloseBear()
        {
            return;
            if (!Communal._b_isBSendOpen)
            {
                if (Communal._i_bType == 1)
                {
                    Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    buzzer.Buzzer_On();

                    Communal._b_isBSendClose = false;
                    Communal._b_isBSendOpen = true;
                }
            }

            if (!Communal._b_isBSendClose)
            {
                if (Communal._i_bType == 2)
                {
                    Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer buzzer = new Lib_Card.ADT8940A1.OutPut.Buzzer.Buzzer_Basic();
                    buzzer.Buzzer_Off();

                    Communal._b_isBSendOpen = false;
                    Communal._b_isBSendClose = true;
                }
            }
        }


        /// <summary>
        /// 回零
        /// </summary>
        /// <returns></returns>
        public static int goHome()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.Home.Home home = new Lib_Card.ADT8940A1.Module.Home.Home_Condition();
                if (-1 == home.Home_XYZ(Lib_Card.Configure.Parameter.Machine_CylinderVersion))
                    return -1;

                return 0;
            }
            else
            {
                try
                {
                    Lib_Log.Log.writeLogException("执行回零方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询 
                    if (!FADM_Object.Communal._b_auto)
                    {
                        for (int i = 0; i < Communal._ia_d900.Length; i++) { Communal._ia_d900[i] = 0; }
                        Communal._b_isUpdateNewData = false;
                        //等待调试页面停止刷新
                        Thread.Sleep(200);
                    }
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int[] ia_array = { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入回零动作编号返回失败,继续写入");
                        Console.WriteLine("写入回零动作编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2"))
                    {
                        throw e;
                    }
                    else
                    {
                        MessageBox.Show(e.ToString());
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 计算目标坐标
        /// </summary>
        /// <param name="i_move_Type">移动类型 0母液瓶 1缸杯位 2 天平位 3 待机位置 4 放盖区 5 泄压区 6:干布区域 7:湿布区域 8:干布夹子 9:湿布夹子 10:吸光度杯位  
        /// 11:抽液针筒  12:洗针筒 13：撑盖位 14：粉筒布夹 15：洗瓶位置 16：开料工位 17：PH计  18：盐度计 19：自动洗瓶洗针筒后存放坐标 20：PH测量液存放位置 21：加水校正位置</param>
        /// <param name="Type">0:气缸上到位 1：气缸中限位</param>
        /// <returns></returns>
        public static int CalTarget(int i_move_Type, int i_no, ref int i_x, ref int i_y)
        {
            try
            {
                Lib_Log.Log.writeLogException("计算坐标");
                Console.WriteLine("计算坐标");
                int i_xPules = 0;
                int i_yPules = 0;
                switch (i_move_Type)
                {
                    case 0:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1 || (!Communal._b_isBalanceInDrip))
                        {
                            //需要把AB助剂区分开
                            if (FADM_Object.Communal._b_isUseABAssistant)
                            {
                                if(i_no <= Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount)
                                {
                                    if (FADM_Object.Communal._b_isNewSet)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                            }
                            else
                            {
                                if (FADM_Object.Communal._b_isNewSet)
                                {
                                    //如果分区域
                                    if (FADM_Object.Communal._b_isPartition)
                                    {
                                        //如果是母液坐标
                                        if (i_no >= FADM_Object.Communal._i_MotherBottleNum)
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_X - (i_no - FADM_Object.Communal._i_MotherBottleNum) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_Y + (i_no - FADM_Object.Communal._i_MotherBottleNum) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                        else
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                                else
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                    Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                        }
                        else
                        {
                            int iNo = i_no;
                            if (FADM_Object.Communal._b_isUseABAssistant)
                            {
                                if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14+ FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                                else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (7+ FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                        ((iNo + (14+ FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14+ FADM_Object.Communal._i_ABAssistantCount)) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + (14+ FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                }
                                else if(Lib_Card.Configure.Parameter.Machine_Bottle_Total -  FADM_Object.Communal._i_ABAssistantCount >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                       ((iNo + (14+ FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                       * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14+ FADM_Object.Communal._i_ABAssistantCount)) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + (14+ FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14 >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                                else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 7 >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                        ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                }
                                else
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                       ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                       * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                        }
                        break;
                    case 1:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                        }
                        else
                        {
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                            {
                                if (i_no == 1)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                }
                                else if (i_no == 2)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                }
                                else if (i_no == 3)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                }
                                else if (i_no == 4)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                }
                                else if (i_no == 5)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                }
                                else if (i_no == 6)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                }
                                else if (i_no == 7)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                }
                                else if (i_no == 8)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                }
                                else if (i_no == 9)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                }
                                else if (i_no == 10)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                }
                                else if (i_no == 11)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                }
                                else if (i_no == 12)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                }
                                else if (i_no == 13)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                }
                                else if (i_no == 14)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                }
                                else if (i_no == 15)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                }
                                else if (i_no == 16)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                }
                                else if (i_no == 17)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                }
                                else if (i_no == 18)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                }
                                else if (i_no == 19)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                }
                                else if (i_no == 20)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                }
                                else if (i_no == 21)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                }
                                else if (i_no == 22)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                }
                                else if (i_no == 23)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                }
                                else if (i_no == 24)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                }
                                else if (i_no == 25)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                }
                                else if (i_no == 26)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                }
                                else if (i_no == 27)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                }
                                else if (i_no == 28)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                }
                                else if (i_no == 29)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                }
                                else if (i_no == 30)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                }
                                else if (i_no == 31)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                }
                                else if (i_no == 32)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                }
                                else if (i_no == 33)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                }
                                else if (i_no == 34)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                }
                                else if (i_no == 35)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                }
                                else if (i_no == 36)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                }
                                else if (i_no == 37)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                }
                                else if (i_no == 38)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                }
                                else if (i_no == 39)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                }
                                else if (i_no == 40)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                }
                                else if (i_no == 41)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                }
                                else if (i_no == 42)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                }
                                else if (i_no == 43)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                }
                                else if (i_no == 44)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                }
                                else if (i_no == 45)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                }
                                else if (i_no == 46)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                }
                                else if (i_no == 47)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                }
                                else if (i_no == 48)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                }
                            }
                            else
                            {
                                if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }

                                }
                            }
                        }
                        break;

                    case 2:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Balance_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Balance_Y;
                        break;
                    case 3:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Standby_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Standby_Y;
                        break;
                    case 4:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalY;
                        }
                        else if (i_no == 7)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalY;
                        }
                        else if (i_no == 8)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalY;
                        }
                        else if (i_no == 9)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalY;
                        }
                        else if (i_no == 10)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalY;
                        }
                        else if (i_no == 11)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalY;
                        }
                        else if (i_no == 12)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalY;
                        }
                        else if (i_no == 13)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalY;
                        }
                        else if (i_no == 14)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalY;
                        }
                        else if (i_no == 15)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalY;
                        }
                        else if (i_no == 16)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalY;
                        }
                        else if (i_no == 17)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalY;
                        }
                        else if (i_no == 18)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalY;
                        }
                        else if (i_no == 19)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalY;
                        }
                        else if (i_no == 20)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalY;
                        }
                        else if (i_no == 21)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalY;
                        }
                        else if (i_no == 22)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalY;
                        }
                        else if (i_no == 23)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalY;
                        }
                        else if (i_no == 24)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalY;
                        }
                        else if (i_no == 25)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalY;
                        }
                        else if (i_no == 26)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalY;
                        }
                        else if (i_no == 27)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalY;
                        }
                        else if (i_no == 28)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalY;
                        }
                        else if (i_no == 29)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalY;
                        }
                        else if (i_no == 30)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalY;
                        }
                        else if (i_no == 31)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalY;
                        }
                        else if (i_no == 32)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalY;
                        }
                        else if (i_no == 33)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalY;
                        }
                        else if (i_no == 34)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalY;
                        }
                        else if (i_no == 35)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalY;
                        }
                        else if (i_no == 36)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalY;
                        }
                        else if (i_no == 37)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalY;
                        }
                        else if (i_no == 38)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalY;
                        }
                        else if (i_no == 39)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalY;
                        }
                        else if (i_no == 40)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalY;
                        }
                        else if (i_no == 41)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalY;
                        }
                        else if (i_no == 42)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalY;
                        }
                        else if (i_no == 43)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalY;
                        }
                        else if (i_no == 44)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalY;
                        }
                        else if (i_no == 45)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalY;
                        }
                        else if (i_no == 46)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalY;
                        }
                        else if (i_no == 47)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalY;
                        }
                        else if (i_no == 48)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalY;
                        }
                        break;
                    case 5:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                        }
                        else
                        {
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                            {
                                if (i_no == 1)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                }
                                else if (i_no == 2)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                }
                                else if (i_no == 3)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                }
                                else if (i_no == 4)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                }
                                else if (i_no == 5)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                }
                                else if (i_no == 6)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                }
                                else if (i_no == 7)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                }
                                else if (i_no == 8)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                }
                                else if (i_no == 9)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                }
                                else if (i_no == 10)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                }
                                else if (i_no == 11)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                }
                                else if (i_no == 12)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                }
                                else if (i_no == 13)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                }
                                else if (i_no == 14)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                }
                                else if (i_no == 15)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                }
                                else if (i_no == 16)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                }
                                else if (i_no == 17)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                }
                                else if (i_no == 18)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                }
                                else if (i_no == 19)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                }
                                else if (i_no == 20)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                }
                                else if (i_no == 21)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                }
                                else if (i_no == 22)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                }
                                else if (i_no == 23)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                }
                                else if (i_no == 24)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                }
                                else if (i_no == 25)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                }
                                else if (i_no == 26)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                }
                                else if (i_no == 27)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                }
                                else if (i_no == 28)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                }
                                else if (i_no == 29)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                }
                                else if (i_no == 30)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                }
                                else if (i_no == 31)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                }
                                else if (i_no == 32)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                }
                                else if (i_no == 33)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                }
                                else if (i_no == 34)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                }
                                else if (i_no == 35)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                }
                                else if (i_no == 36)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                }
                                else if (i_no == 37)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                }
                                else if (i_no == 38)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                }
                                else if (i_no == 39)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                }
                                else if (i_no == 40)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                }
                                else if (i_no == 41)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                }
                                else if (i_no == 42)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                }
                                else if (i_no == 43)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                }
                                else if (i_no == 44)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                }
                                else if (i_no == 45)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                }
                                else if (i_no == 46)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                }
                                else if (i_no == 47)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                }
                                else if (i_no == 48)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                }
                            }
                            else
                            {
                                if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }

                                }
                            }
                        }
                        i_xPules -= Lib_Card.Configure.Parameter.Coordinate_Cup_Decompression;
                        break;
                    case 6:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMax)
                        {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalY;

                            
                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalY;


                        }

                        break;
                    case 7:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMax)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalY;


                        }

                        break;
                    case 8:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y;
                        break;
                    case 9:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y;
                        break;
                    case 10:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_Y;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_Y;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_Y;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_Y;
                        }
                        break;
                    case 11:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_Y;
                        break;
                    case 12:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Wash_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Wash_Y;
                        break;
                    case 13:
                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                        {
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                            }
                            else if (i_no == 5)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                            }
                            else if (i_no == 6)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                            }
                            else if (i_no == 7)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                            }
                            else if (i_no == 8)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                            }
                            else if (i_no == 9)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                            }
                            else if (i_no == 10)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                            }
                            else if (i_no == 11)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                            }
                            else if (i_no == 12)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                            }
                            else if (i_no == 13)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                            }
                            else if (i_no == 14)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                            }
                            else if (i_no == 15)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                            }
                            else if (i_no == 16)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                            }
                            else if (i_no == 17)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                            }
                            else if (i_no == 18)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                            }
                            else if (i_no == 19)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                            }
                            else if (i_no == 20)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                            }
                            else if (i_no == 21)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                            }
                            else if (i_no == 22)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                            }
                            else if (i_no == 23)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                            }
                            else if (i_no == 24)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                            }
                            else if (i_no == 25)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                            }
                            else if (i_no == 26)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                            }
                            else if (i_no == 27)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                            }
                            else if (i_no == 28)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                            }
                            else if (i_no == 29)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                            }
                            else if (i_no == 30)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                            }
                            else if (i_no == 31)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                            }
                            else if (i_no == 32)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                            }
                            else if (i_no == 33)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                            }
                            else if (i_no == 34)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                            }
                            else if (i_no == 35)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                            }
                            else if (i_no == 36)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                            }
                            else if (i_no == 37)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                            }
                            else if (i_no == 38)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                            }
                            else if (i_no == 39)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                            }
                            else if (i_no == 40)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                            }
                            else if (i_no == 41)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                            }
                            else if (i_no == 42)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                            }
                            else if (i_no == 43)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                            }
                            else if (i_no == 44)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                            }
                            else if (i_no == 45)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                            }
                            else if (i_no == 46)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                            }
                            else if (i_no == 47)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                            }
                            else if (i_no == 48)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                            }

                            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
                            {
                                i_yPules += Lib_Card.Configure.Parameter.Other_SupportCoverY;
                            }
                            else
                            {
                                i_yPules -= Lib_Card.Configure.Parameter.Other_SupportCoverY;
                            }
                        }
                        break;
                    case 14:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y;
                        break;
                    case 15:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y;
                        break;
                    case 16:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY;
                        }
                        break;
                    case 17:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY;
                        break;
                    case 18:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY;
                        break;
                    case 19:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY;
                        }
                        else if (i_no == 7)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalY;
                        }
                        else if (i_no == 8)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalY;
                        }
                        else if (i_no == 9)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalY;
                        }
                        else if (i_no == 10)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalY;
                        }
                        else if (i_no == 11)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalY;
                        }
                        break;
                    case 20:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMax)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalY;


                        }

                        break;
                    case 21:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalY;
                        break;
                    default:
                        throw new Exception("5");
                }
                Lib_Log.Log.writeLogException("计算脉冲是iXPules=" + i_xPules.ToString() + "计算iYPules=" + i_yPules.ToString());
                Console.WriteLine("计算脉冲是iXPules=" + i_xPules.ToString() + "计算iYPules=" + i_yPules.ToString());
                //int d_1 = 0;
                //if (i_xPules > 65536)
                //{
                //    d_1 = i_xPules / 65536;
                //    i_xPules = i_xPules % 65536;
                //}
                //int d_2 = 0;
                //if (i_yPules > 65536)
                //{
                //    d_2 = i_yPules / 65536;
                //    i_yPules = i_yPules % 65536;
                //}
                i_x = i_xPules;
                i_y = i_yPules;


                return 0;
            }
            catch (Exception e)
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
                if (e.Message.Equals("4") || e.Message.Equals("5") || e.Message.Equals("-2") || e.Message.Equals("6"))
                {
                    throw e;
                }
                else
                {
                    MessageBox.Show(e.ToString());
                    throw new Exception("-1");
                }
            }
            finally
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
            }
        }

        /// <summary>
        /// 定点移动
        /// </summary>
        /// <param name="i_move_Type">移动类型 0母液瓶 1缸杯位 2 天平位 3 待机位置 4 放盖区 5 泄压区 6:干布区域 7:湿布区域 8:干布夹子 9:湿布夹子 10:吸光度杯位  
        /// 11:抽液针筒  12:洗针筒 13：撑盖位 14：粉筒布夹 15：洗瓶位置 16：开料工位 17：PH计  18：盐度计 19：自动洗瓶洗针筒后存放坐标 20：PH测量液存放位置 21：加水校正位置</param>
        /// <param name="i_type">0:气缸上到位 1：气缸中限位</param>
        /// <returns></returns>
        public static int TargetMove(int i_move_Type, int i_no, int i_type)
        {
            if (!Communal._b_isMove)
            {
                Communal._b_isMove = true;
            }

            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();

                int i_xPules = 0;
                int i_yPules = 0;
                switch (i_move_Type)
                {
                    case 0:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1 || (!Communal._b_isBalanceInDrip))
                        {
                            //需要把AB助剂区分开
                            if (FADM_Object.Communal._b_isUseABAssistant)
                            {
                                if (i_no <= Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount)
                                {
                                    if (FADM_Object.Communal._b_isNewSet)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                            }
                            else
                            {
                                if (FADM_Object.Communal._b_isNewSet)
                                {
                                    //如果分区域
                                    if (FADM_Object.Communal._b_isPartition)
                                    {
                                        //如果是母液坐标
                                        if (i_no >= FADM_Object.Communal._i_MotherBottleNum)
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_X - (i_no - FADM_Object.Communal._i_MotherBottleNum) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_Y + (i_no - FADM_Object.Communal._i_MotherBottleNum) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                        else
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                                else
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                    Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                        }
                        else
                        {
                            int iNo = i_no;
                            if (FADM_Object.Communal._b_isUseABAssistant)
                            {
                                if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                                else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (7 + FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                        ((iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount)) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                }
                                else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                       ((iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                       * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount)) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                            else
                            {
                                if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14 >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                                else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 7 >= iNo)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                        ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                }
                                else
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                       ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                       * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                        ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                        (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                        * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                }
                            }
                        }
                        break;
                    case 1:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                        }
                        else
                        {
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                            {
                                if (i_no == 1)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                }
                                else if (i_no == 2)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                }
                                else if (i_no == 3)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                }
                                else if (i_no == 4)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                }
                                else if (i_no == 5)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                }
                                else if (i_no == 6)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                }
                                else if (i_no == 7)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                }
                                else if (i_no == 8)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                }
                                else if (i_no == 9)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                }
                                else if (i_no == 10)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                }
                                else if (i_no == 11)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                }
                                else if (i_no == 12)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                }
                                else if (i_no == 13)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                }
                                else if (i_no == 14)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                }
                                else if (i_no == 15)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                }
                                else if (i_no == 16)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                }
                                else if (i_no == 17)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                }
                                else if (i_no == 18)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                }
                                else if (i_no == 19)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                }
                                else if (i_no == 20)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                }
                                else if (i_no == 21)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                }
                                else if (i_no == 22)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                }
                                else if (i_no == 23)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                }
                                else if (i_no == 24)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                }
                                else if (i_no == 25)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                }
                                else if (i_no == 26)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                }
                                else if (i_no == 27)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                }
                                else if (i_no == 28)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                }
                                else if (i_no == 29)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                }
                                else if (i_no == 30)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                }
                                else if (i_no == 31)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                }
                                else if (i_no == 32)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                }
                                else if (i_no == 33)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                }
                                else if (i_no == 34)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                }
                                else if (i_no == 35)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                }
                                else if (i_no == 36)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                }
                                else if (i_no == 37)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                }
                                else if (i_no == 38)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                }
                                else if (i_no == 39)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                }
                                else if (i_no == 40)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                }
                                else if (i_no == 41)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                }
                                else if (i_no == 42)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                }
                                else if (i_no == 43)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                }
                                else if (i_no == 44)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                }
                                else if (i_no == 45)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                }
                                else if (i_no == 46)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                }
                                else if (i_no == 47)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                }
                                else if (i_no == 48)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                }
                            }
                            else
                            {
                                if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }

                                }
                            }
                        }
                        break;

                    case 2:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Balance_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Balance_Y;
                        break;
                    case 3:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Standby_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Standby_Y;
                        break;
                    case 4:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalY;
                        }
                        else if (i_no == 7)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalY;
                        }
                        else if (i_no == 8)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalY;
                        }
                        else if (i_no == 9)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalY;
                        }
                        else if (i_no == 10)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalY;
                        }
                        else if (i_no == 11)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalY;
                        }
                        else if (i_no == 12)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalY;
                        }
                        else if (i_no == 13)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalY;
                        }
                        else if (i_no == 14)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalY;
                        }
                        else if (i_no == 15)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalY;
                        }
                        else if (i_no == 16)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalY;
                        }
                        else if (i_no == 17)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalY;
                        }
                        else if (i_no == 18)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalY;
                        }
                        else if (i_no == 19)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalY;
                        }
                        else if (i_no == 20)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalY;
                        }
                        else if (i_no == 21)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalY;
                        }
                        else if (i_no == 22)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalY;
                        }
                        else if (i_no == 23)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalY;
                        }
                        else if (i_no == 24)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalY;
                        }
                        else if (i_no == 25)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalY;
                        }
                        else if (i_no == 26)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalY;
                        }
                        else if (i_no == 27)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalY;
                        }
                        else if (i_no == 28)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalY;
                        }
                        else if (i_no == 29)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalY;
                        }
                        else if (i_no == 30)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalY;
                        }
                        else if (i_no == 31)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalY;
                        }
                        else if (i_no == 32)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalY;
                        }
                        else if (i_no == 33)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalY;
                        }
                        else if (i_no == 34)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalY;
                        }
                        else if (i_no == 35)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalY;
                        }
                        else if (i_no == 36)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalY;
                        }
                        else if (i_no == 37)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalY;
                        }
                        else if (i_no == 38)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalY;
                        }
                        else if (i_no == 39)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalY;
                        }
                        else if (i_no == 40)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalY;
                        }
                        else if (i_no == 41)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalY;
                        }
                        else if (i_no == 42)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalY;
                        }
                        else if (i_no == 43)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalY;
                        }
                        else if (i_no == 44)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalY;
                        }
                        else if (i_no == 45)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalY;
                        }
                        else if (i_no == 46)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalY;
                        }
                        else if (i_no == 47)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalY;
                        }
                        else if (i_no == 48)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalY;
                        }
                        break;
                    case 5:
                        if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                        {
                            throw new Exception("4");
                        }
                        if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                        }
                        else
                        {
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                            {
                                if (i_no == 1)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                }
                                else if (i_no == 2)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                }
                                else if (i_no == 3)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                }
                                else if (i_no == 4)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                }
                                else if (i_no == 5)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                }
                                else if (i_no == 6)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                }
                                else if (i_no == 7)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                }
                                else if (i_no == 8)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                }
                                else if (i_no == 9)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                }
                                else if (i_no == 10)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                }
                                else if (i_no == 11)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                }
                                else if (i_no == 12)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                }
                                else if (i_no == 13)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                }
                                else if (i_no == 14)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                }
                                else if (i_no == 15)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                }
                                else if (i_no == 16)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                }
                                else if (i_no == 17)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                }
                                else if (i_no == 18)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                }
                                else if (i_no == 19)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                }
                                else if (i_no == 20)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                }
                                else if (i_no == 21)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                }
                                else if (i_no == 22)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                }
                                else if (i_no == 23)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                }
                                else if (i_no == 24)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                }
                                else if (i_no == 25)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                }
                                else if (i_no == 26)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                }
                                else if (i_no == 27)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                }
                                else if (i_no == 28)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                }
                                else if (i_no == 29)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                }
                                else if (i_no == 30)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                }
                                else if (i_no == 31)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                }
                                else if (i_no == 32)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                }
                                else if (i_no == 33)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                }
                                else if (i_no == 34)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                }
                                else if (i_no == 35)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                }
                                else if (i_no == 36)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                }
                                else if (i_no == 37)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                }
                                else if (i_no == 38)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                }
                                else if (i_no == 39)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                }
                                else if (i_no == 40)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                }
                                else if (i_no == 41)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                }
                                else if (i_no == 42)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                }
                                else if (i_no == 43)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                }
                                else if (i_no == 44)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                }
                                else if (i_no == 45)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                }
                                else if (i_no == 46)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                }
                                else if (i_no == 47)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                }
                                else if (i_no == 48)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                }
                            }
                            else
                            {
                                if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域1
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域2
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域3
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                    }
                                    else
                                    {
                                        //在区域4
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                    }
                                }
                                else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域5
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                      ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }
                                    else
                                    {
                                        //在区域6
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                    }

                                }
                            }
                        }
                        i_xPules -= Lib_Card.Configure.Parameter.Coordinate_Cup_Decompression;
                        break;
                    case 6:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMax)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalY;


                        }

                        break;
                    case 7:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMax)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalY;


                        }

                        break;
                    case 8:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y;
                        break;
                    case 9:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y;
                        break;
                    case 10:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_Y;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_Y;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_Y;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_Y;
                        }
                        break;
                    case 11:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_Y;
                        break;
                    case 12:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Wash_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Wash_Y;
                        break;
                    case 13:
                        if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3
                                || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                        {
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                            }
                            else if (i_no == 5)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                            }
                            else if (i_no == 6)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                            }
                            else if (i_no == 7)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                            }
                            else if (i_no == 8)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                            }
                            else if (i_no == 9)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                            }
                            else if (i_no == 10)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                            }
                            else if (i_no == 11)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                            }
                            else if (i_no == 12)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                            }
                            else if (i_no == 13)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                            }
                            else if (i_no == 14)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                            }
                            else if (i_no == 15)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                            }
                            else if (i_no == 16)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                            }
                            else if (i_no == 17)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                            }
                            else if (i_no == 18)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                            }
                            else if (i_no == 19)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                            }
                            else if (i_no == 20)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                            }
                            else if (i_no == 21)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                            }
                            else if (i_no == 22)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                            }
                            else if (i_no == 23)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                            }
                            else if (i_no == 24)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                            }
                            else if (i_no == 25)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                            }
                            else if (i_no == 26)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                            }
                            else if (i_no == 27)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                            }
                            else if (i_no == 28)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                            }
                            else if (i_no == 29)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                            }
                            else if (i_no == 30)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                            }
                            else if (i_no == 31)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                            }
                            else if (i_no == 32)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                            }
                            else if (i_no == 33)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                            }
                            else if (i_no == 34)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                            }
                            else if (i_no == 35)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                            }
                            else if (i_no == 36)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                            }
                            else if (i_no == 37)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                            }
                            else if (i_no == 38)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                            }
                            else if (i_no == 39)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                            }
                            else if (i_no == 40)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                            }
                            else if (i_no == 41)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                            }
                            else if (i_no == 42)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                            }
                            else if (i_no == 43)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                            }
                            else if (i_no == 44)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                            }
                            else if (i_no == 45)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                            }
                            else if (i_no == 46)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                            }
                            else if (i_no == 47)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                            }
                            else if (i_no == 48)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                            }

                            i_yPules += Lib_Card.Configure.Parameter.Other_SupportCoverY;
                        }
                        break;
                    case 14:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y;
                        break;
                    case 15:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y;
                        break;
                    case 16:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY;
                        }
                        break;
                    case 17:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY;
                        break;
                    case 18:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY;
                        break;
                    case 19:
                        if (i_no == 1)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY;
                        }
                        else if (i_no == 2)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY;
                        }
                        else if (i_no == 3)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY;
                        }
                        else if (i_no == 4)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY;
                        }
                        else if (i_no == 5)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY;
                        }
                        else if (i_no == 6)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY;
                        }
                        else if (i_no == 7)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalY;
                        }
                        else if (i_no == 8)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalY;
                        }
                        else if (i_no == 9)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalY;
                        }
                        else if (i_no == 10)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalY;
                        }
                        else if (i_no == 11)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalY;
                        }
                        break;
                    case 20:
                        if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMax)
                        {
                            //在区域1
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMax)
                        {

                            //在区域2
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_X +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalY;


                        }
                        else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMax)
                        {

                            //在区域3
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_X +
                                 ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalX;

                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_Y +
                                ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalY;


                        }

                        break;
                    case 21:
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalX;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalY;
                        break;
                    default:
                        throw new Exception("5");
                }
                Lib_Log.Log.writeLogException("脉冲是iXPules=" + i_xPules.ToString() + "iYPules=" + i_yPules.ToString());
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_no, i_xPules,i_yPules,i_move_Type);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                return 0;
            }
            else
            {
                try
                {
                    
                    Lib_Log.Log.writeLogException("开始定点移动");
                    Console.WriteLine("开始定点移动");
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    if (!FADM_Object.Communal._b_auto)
                    {
                        for (int i = 0; i < Communal._ia_d900.Length; i++) { Communal._ia_d900[i] = 0; }
                        Communal._b_isUpdateNewData = false;
                        //等待调试页面停止刷新
                        Thread.Sleep(200);
                    }
                    ClearSuccessState();//先清除标志位

                    Console.WriteLine("清除完标志位");
                    Lib_Log.Log.writeLogException("清除标志位结束");
                lableTop:
                Label1:
                    /*if (!FADM_Object.Communal._b_auto) {//在手动页面 等待手动页面退出
                        goto Label1;
                    }*/
                    int i_xPules = 0;
                    int i_yPules = 0;
                    switch (i_move_Type)
                    {
                        case 0:
                            if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Bottle_Total)
                            {
                                throw new Exception("4");
                            }
                            if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1 || (!Communal._b_isBalanceInDrip))
                            {
                                //需要把AB助剂区分开
                                if (FADM_Object.Communal._b_isUseABAssistant)
                                {
                                    if (i_no <= Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount)
                                    {
                                        if (FADM_Object.Communal._b_isNewSet)
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                        else
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                    }
                                }
                                else
                                {
                                    if (FADM_Object.Communal._b_isNewSet)
                                    {
                                        //如果分区域
                                        if (FADM_Object.Communal._b_isPartition)
                                        {
                                            //如果是母液坐标
                                            if (i_no >= FADM_Object.Communal._i_MotherBottleNum)
                                            {
                                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_X - (i_no - FADM_Object.Communal._i_MotherBottleNum) %
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_MotherBottle_Y + (i_no - FADM_Object.Communal._i_MotherBottleNum) /
                                                    Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            }
                                            else
                                            {
                                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                                    Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            }
                                        }
                                        else
                                        {
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (i_no - 1) /
                                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        }
                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (i_no - 1) %
                                        Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (i_no - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                            }
                            else
                            {
                                int iNo = i_no;
                                if (FADM_Object.Communal._b_isUseABAssistant)
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                    else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - (7 + FADM_Object.Communal._i_ABAssistantCount) >= iNo)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                            ((iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                            ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount)) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                            (iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                    }
                                    else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - FADM_Object.Communal._i_ABAssistantCount >= iNo)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                           ((iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                           * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                            ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - (14 + FADM_Object.Communal._i_ABAssistantCount)) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                            (iNo + (14 + FADM_Object.Communal._i_ABAssistantCount) - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                                else
                                {
                                    if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14 >= iNo)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                    else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 7 >= iNo)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                            ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                            ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                            (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                                    }
                                    else
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                           ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                                           * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                            ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                            Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                            (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                            * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                                    }
                                }
                            }
                            break;
                        case 1:
                            if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                            {
                                throw new Exception("4");
                            }
                            if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                            {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                            }
                            else
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                                {
                                    if (i_no == 1)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                    }
                                    else if (i_no == 2)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                    }
                                    else if (i_no == 3)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                    }
                                    else if (i_no == 4)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                    }
                                    else if (i_no == 5)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                    }
                                    else if (i_no == 6)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                    }
                                    else if (i_no == 7)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                    }
                                    else if (i_no == 8)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                    }
                                    else if (i_no == 9)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                    }
                                    else if (i_no == 10)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                    }
                                    else if (i_no == 11)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                    }
                                    else if (i_no == 12)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                    }
                                    else if (i_no == 13)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                    }
                                    else if (i_no == 14)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                    }
                                    else if (i_no == 15)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                    }
                                    else if (i_no == 16)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                    }
                                    else if (i_no == 17)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                    }
                                    else if (i_no == 18)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                    }
                                    else if (i_no == 19)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                    }
                                    else if (i_no == 20)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                    }
                                    else if (i_no == 21)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                    }
                                    else if (i_no == 22)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                    }
                                    else if (i_no == 23)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                    }
                                    else if (i_no == 24)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                    }
                                    else if (i_no == 25)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                    }
                                    else if (i_no == 26)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                    }
                                    else if (i_no == 27)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                    }
                                    else if (i_no == 28)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                    }
                                    else if (i_no == 29)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                    }
                                    else if (i_no == 30)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                    }
                                    else if (i_no == 31)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                    }
                                    else if (i_no == 32)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                    }
                                    else if (i_no == 33)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                    }
                                    else if (i_no == 34)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                    }
                                    else if (i_no == 35)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                    }
                                    else if (i_no == 36)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                    }
                                    else if (i_no == 37)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                    }
                                    else if (i_no == 38)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                    }
                                    else if (i_no == 39)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                    }
                                    else if (i_no == 40)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                    }
                                    else if (i_no == 41)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                    }
                                    else if (i_no == 42)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                    }
                                    else if (i_no == 43)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                    }
                                    else if (i_no == 44)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                    }
                                    else if (i_no == 45)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                    }
                                    else if (i_no == 46)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                    }
                                    else if (i_no == 47)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                    }
                                    else if (i_no == 48)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                        {
                                            //在区域1
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域1
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                        {
                                            //在区域2
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域2
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                        {
                                            //在区域3
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域3
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                        {
                                            //在区域4
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域4
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                        {
                                            //在区域5
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                        }
                                        else
                                        {
                                            //在区域5
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                        }
                                    }
                                    else
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                        {
                                            //在区域6
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                        }
                                        else
                                        {
                                            //在区域6
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                         ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                        }

                                    }
                                }
                            }
                            break;

                        case 2:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Balance_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Balance_Y;
                            break;
                        case 3:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Standby_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Standby_Y;
                            break;
                        case 4:
                            if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                            {
                                throw new Exception("4");
                            }
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover1_IntervalY;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover2_IntervalY;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover3_IntervalY;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover4_IntervalY;
                            }
                            else if (i_no == 5)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover5_IntervalY;
                            }
                            else if (i_no == 6)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover6_IntervalY;
                            }
                            else if (i_no == 7)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover7_IntervalY;
                            }
                            else if (i_no == 8)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover8_IntervalY;
                            }
                            else if (i_no == 9)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover9_IntervalY;
                            }
                            else if (i_no == 10)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover10_IntervalY;
                            }
                            else if (i_no == 11)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover11_IntervalY;
                            }
                            else if (i_no == 12)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover12_IntervalY;
                            }
                            else if (i_no == 13)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover13_IntervalY;
                            }
                            else if (i_no == 14)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover14_IntervalY;
                            }
                            else if (i_no == 15)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover15_IntervalY;
                            }
                            else if (i_no == 16)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover16_IntervalY;
                            }
                            else if (i_no == 17)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover17_IntervalY;
                            }
                            else if (i_no == 18)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover18_IntervalY;
                            }
                            else if (i_no == 19)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover19_IntervalY;
                            }
                            else if (i_no == 20)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover20_IntervalY;
                            }
                            else if (i_no == 21)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover21_IntervalY;
                            }
                            else if (i_no == 22)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover22_IntervalY;
                            }
                            else if (i_no == 23)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover23_IntervalY;
                            }
                            else if (i_no == 24)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover24_IntervalY;
                            }
                            else if (i_no == 25)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover25_IntervalY;
                            }
                            else if (i_no == 26)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover26_IntervalY;
                            }
                            else if (i_no == 27)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover27_IntervalY;
                            }
                            else if (i_no == 28)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover28_IntervalY;
                            }
                            else if (i_no == 29)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover29_IntervalY;
                            }
                            else if (i_no == 30)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover30_IntervalY;
                            }
                            else if (i_no == 31)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover31_IntervalY;
                            }
                            else if (i_no == 32)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover32_IntervalY;
                            }
                            else if (i_no == 33)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover33_IntervalY;
                            }
                            else if (i_no == 34)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover34_IntervalY;
                            }
                            else if (i_no == 35)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover35_IntervalY;
                            }
                            else if (i_no == 36)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover36_IntervalY;
                            }
                            else if (i_no == 37)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover37_IntervalY;
                            }
                            else if (i_no == 38)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover38_IntervalY;
                            }
                            else if (i_no == 39)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover39_IntervalY;
                            }
                            else if (i_no == 40)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover40_IntervalY;
                            }
                            else if (i_no == 41)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover41_IntervalY;
                            }
                            else if (i_no == 42)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover42_IntervalY;
                            }
                            else if (i_no == 43)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover43_IntervalY;
                            }
                            else if (i_no == 44)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover44_IntervalY;
                            }
                            else if (i_no == 45)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover45_IntervalY;
                            }
                            else if (i_no == 46)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover46_IntervalY;
                            }
                            else if (i_no == 47)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover47_IntervalY;
                            }
                            else if (i_no == 48)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_CupCover48_IntervalY;
                            }
                            break;
                        case 5:
                            if (0 >= i_no || i_no > Lib_Card.Configure.Parameter.Machine_Cup_Total)
                            {
                                throw new Exception("4");
                            }
                            if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1)
                            {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                            }
                            else
                            {
                                if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 0 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                                {
                                    if (i_no == 1)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                    }
                                    else if (i_no == 2)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                    }
                                    else if (i_no == 3)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                    }
                                    else if (i_no == 4)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                    }
                                    else if (i_no == 5)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                    }
                                    else if (i_no == 6)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                    }
                                    else if (i_no == 7)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                    }
                                    else if (i_no == 8)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                    }
                                    else if (i_no == 9)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                    }
                                    else if (i_no == 10)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                    }
                                    else if (i_no == 11)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                    }
                                    else if (i_no == 12)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                    }
                                    else if (i_no == 13)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                    }
                                    else if (i_no == 14)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                    }
                                    else if (i_no == 15)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                    }
                                    else if (i_no == 16)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                    }
                                    else if (i_no == 17)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                    }
                                    else if (i_no == 18)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                    }
                                    else if (i_no == 19)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                    }
                                    else if (i_no == 20)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                    }
                                    else if (i_no == 21)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                    }
                                    else if (i_no == 22)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                    }
                                    else if (i_no == 23)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                    }
                                    else if (i_no == 24)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                    }
                                    else if (i_no == 25)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                    }
                                    else if (i_no == 26)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                    }
                                    else if (i_no == 27)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                    }
                                    else if (i_no == 28)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                    }
                                    else if (i_no == 29)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                    }
                                    else if (i_no == 30)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                    }
                                    else if (i_no == 31)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                    }
                                    else if (i_no == 32)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                    }
                                    else if (i_no == 33)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                    }
                                    else if (i_no == 34)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                    }
                                    else if (i_no == 35)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                    }
                                    else if (i_no == 36)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                    }
                                    else if (i_no == 37)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                    }
                                    else if (i_no == 38)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                    }
                                    else if (i_no == 39)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                    }
                                    else if (i_no == 40)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                    }
                                    else if (i_no == 41)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                    }
                                    else if (i_no == 42)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                    }
                                    else if (i_no == 43)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                    }
                                    else if (i_no == 44)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                    }
                                    else if (i_no == 45)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                    }
                                    else if (i_no == 46)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                    }
                                    else if (i_no == 47)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                    }
                                    else if (i_no == 48)
                                    {
                                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                    }
                                }
                                else
                                {
                                    if (i_no >= Lib_Card.Configure.Parameter.Machine_Area1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area1_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area1_Layout == 1)
                                        {
                                            //在区域1
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y +
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Row) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域1
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area1_X -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) % Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalX;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area1_Y -
                                                ((i_no - Lib_Card.Configure.Parameter.Machine_Area1_CupMin) / Lib_Card.Configure.Parameter.Machine_Area1_Col) * Lib_Card.Configure.Parameter.Coordinate_Area1_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area2_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area2_Layout == 1)
                                        {
                                            //在区域2
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Row) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域2
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area2_X -
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) % Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area2_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area2_CupMin) / Lib_Card.Configure.Parameter.Machine_Area2_Col) * Lib_Card.Configure.Parameter.Coordinate_Area2_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area3_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area3_Layout == 1)
                                        {
                                            //在区域3
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Row) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域3
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area3_X -
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) % Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area3_Y -
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area3_CupMin) / Lib_Card.Configure.Parameter.Machine_Area3_Col) * Lib_Card.Configure.Parameter.Coordinate_Area3_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area4_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area4_Layout == 1)
                                        {
                                            //在区域4
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Row) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;

                                        }
                                        else
                                        {
                                            //在区域4
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area4_X +
                                              ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) % Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area4_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area4_CupMin) / Lib_Card.Configure.Parameter.Machine_Area4_Col) * Lib_Card.Configure.Parameter.Coordinate_Area4_IntervalY;
                                        }
                                    }
                                    else if (i_no >= Lib_Card.Configure.Parameter.Machine_Area5_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_Area5_CupMax)
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area5_Layout == 1)
                                        {
                                            //在区域5
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Row) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                        }
                                        else
                                        {
                                            //在区域5
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area5_X +
                                          ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) % Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area5_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area5_CupMin) / Lib_Card.Configure.Parameter.Machine_Area5_Col) * Lib_Card.Configure.Parameter.Coordinate_Area5_IntervalY;
                                        }
                                    }
                                    else
                                    {
                                        if (Lib_Card.Configure.Parameter.Machine_Area6_Layout == 1)
                                        {
                                            //在区域6
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X -
                                        ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;

                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Row) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                        }
                                        else
                                        {
                                            //在区域6
                                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Area6_X +
                                         ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) % Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalX;
                                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Area6_Y +
                                            ((i_no - Lib_Card.Configure.Parameter.Machine_Area6_CupMin) / Lib_Card.Configure.Parameter.Machine_Area6_Col) * Lib_Card.Configure.Parameter.Coordinate_Area6_IntervalY;
                                        }

                                    }
                                }
                            }
                            i_xPules -= Lib_Card.Configure.Parameter.Coordinate_Cup_Decompression;
                            break;
                        case 6:
                            if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMax)
                            {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth1_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMax)
                            {

                                //在区域2
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth2_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMax)
                            {

                                //在区域3
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaDryCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaDryCloth3_IntervalY;


                            }

                            break;
                        case 7:
                            if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMax)
                            {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth1_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth1_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMax)
                            {

                                //在区域2
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth2_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth2_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMax)
                            {

                                //在区域3
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) % Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_CupMin) / Lib_Card.Configure.Parameter.Machine_AreaWetCloth3_Row) * Lib_Card.Configure.Parameter.Coordinate_AreaWetCloth3_IntervalY;


                            }

                            break;
                        case 8:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_DryClamp_Y;
                            break;
                        case 9:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_WetClamp_Y;
                            break;
                        case 10:
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_X;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs1_Y;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_X;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs2_Y;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_X;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs3_Y;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_X;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Abs4_Y;
                            }
                            break;
                        case 11:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_Y;
                            break;
                        case 12:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Wash_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Wash_Y;
                            break;
                        case 13:
                            if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(i_no) && (SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 1
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 2 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 3
                                    || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 4 || SmartDyeing.FADM_Object.Communal._dic_dyeType[i_no] == 5))
                            {
                                if (i_no == 1)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup1_IntervalY;
                                }
                                else if (i_no == 2)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup2_IntervalY;
                                }
                                else if (i_no == 3)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup3_IntervalY;
                                }
                                else if (i_no == 4)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup4_IntervalY;
                                }
                                else if (i_no == 5)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup5_IntervalY;
                                }
                                else if (i_no == 6)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup6_IntervalY;
                                }
                                else if (i_no == 7)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup7_IntervalY;
                                }
                                else if (i_no == 8)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup8_IntervalY;
                                }
                                else if (i_no == 9)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup9_IntervalY;
                                }
                                else if (i_no == 10)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup10_IntervalY;
                                }
                                else if (i_no == 11)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup11_IntervalY;
                                }
                                else if (i_no == 12)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup12_IntervalY;
                                }
                                else if (i_no == 13)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup13_IntervalY;
                                }
                                else if (i_no == 14)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup14_IntervalY;
                                }
                                else if (i_no == 15)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup15_IntervalY;
                                }
                                else if (i_no == 16)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup16_IntervalY;
                                }
                                else if (i_no == 17)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup17_IntervalY;
                                }
                                else if (i_no == 18)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup18_IntervalY;
                                }
                                else if (i_no == 19)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup19_IntervalY;
                                }
                                else if (i_no == 20)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup20_IntervalY;
                                }
                                else if (i_no == 21)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup21_IntervalY;
                                }
                                else if (i_no == 22)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup22_IntervalY;
                                }
                                else if (i_no == 23)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup23_IntervalY;
                                }
                                else if (i_no == 24)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup24_IntervalY;
                                }
                                else if (i_no == 25)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup25_IntervalY;
                                }
                                else if (i_no == 26)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup26_IntervalY;
                                }
                                else if (i_no == 27)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup27_IntervalY;
                                }
                                else if (i_no == 28)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup28_IntervalY;
                                }
                                else if (i_no == 29)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup29_IntervalY;
                                }
                                else if (i_no == 30)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup30_IntervalY;
                                }
                                else if (i_no == 31)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup31_IntervalY;
                                }
                                else if (i_no == 32)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup32_IntervalY;
                                }
                                else if (i_no == 33)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup33_IntervalY;
                                }
                                else if (i_no == 34)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup34_IntervalY;
                                }
                                else if (i_no == 35)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup35_IntervalY;
                                }
                                else if (i_no == 36)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup36_IntervalY;
                                }
                                else if (i_no == 37)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup37_IntervalY;
                                }
                                else if (i_no == 38)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup38_IntervalY;
                                }
                                else if (i_no == 39)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup39_IntervalY;
                                }
                                else if (i_no == 40)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup40_IntervalY;
                                }
                                else if (i_no == 41)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup41_IntervalY;
                                }
                                else if (i_no == 42)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup42_IntervalY;
                                }
                                else if (i_no == 43)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup43_IntervalY;
                                }
                                else if (i_no == 44)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup44_IntervalY;
                                }
                                else if (i_no == 45)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup45_IntervalY;
                                }
                                else if (i_no == 46)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup46_IntervalY;
                                }
                                else if (i_no == 47)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup47_IntervalY;
                                }
                                else if (i_no == 48)
                                {
                                    i_xPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalX;
                                    i_yPules = Lib_Card.Configure.Parameter.Coordinate_Cup48_IntervalY;
                                }

                                i_yPules -= Lib_Card.Configure.Parameter.Other_SupportCoverY;
                            }
                            break;
                        case 14:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PowerClamp_Y;
                            break;
                        case 15:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_X;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_WashBottle_Y;
                            break;
                        case 16:
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew1_IntervalY;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew2_IntervalY;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew3_IntervalY;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew4_IntervalY;
                            }
                            else if (i_no == 5)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew5_IntervalY;
                            }
                            else if (i_no == 6)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_Brew6_IntervalY;
                            }
                            break;
                        case 17:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_PH_IntervalY;
                            break;
                        case 18:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Salinity_IntervalY;
                            break;
                        case 19:
                            if (i_no == 1)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes1_IntervalY;
                            }
                            else if (i_no == 2)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes2_IntervalY;
                            }
                            else if (i_no == 3)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes3_IntervalY;
                            }
                            else if (i_no == 4)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes4_IntervalY;
                            }
                            else if (i_no == 5)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes5_IntervalY;
                            }
                            else if (i_no == 6)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes6_IntervalY;
                            }
                            else if (i_no == 7)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes7_IntervalY;
                            }
                            else if (i_no == 8)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes8_IntervalY;
                            }
                            else if (i_no == 9)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes9_IntervalY;
                            }
                            else if (i_no == 10)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes10_IntervalY;
                            }
                            else if (i_no == 11)
                            {
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalX;
                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_PutSyringes11_IntervalY;
                            }
                            break;
                        case 20:
                            if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH1_CupMax)
                            {
                                //在区域1
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH1_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH1_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH1_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH1_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH2_CupMax)
                            {

                                //在区域2
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_X +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH2_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH2_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH2_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH2_IntervalY;


                            }
                            else if (i_no >= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin && i_no <= Lib_Card.Configure.Parameter.Machine_TestPH3_CupMax)
                            {

                                //在区域3
                                i_xPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_X +
                                     ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) % Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalX;

                                i_yPules = Lib_Card.Configure.Parameter.Coordinate_TestPH3_Y +
                                    ((i_no - Lib_Card.Configure.Parameter.Machine_TestPH3_CupMin) / Lib_Card.Configure.Parameter.Machine_TestPH3_Row) * Lib_Card.Configure.Parameter.Coordinate_TestPH3_IntervalY;


                            }

                            break;
                        case 21:
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalX;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_AddWaterCorrecting_IntervalY;
                            break;
                        default:
                            throw new Exception("5");
                    }
                    Lib_Log.Log.writeLogException("脉冲是iXPules=" + i_xPules.ToString() + "iYPules=" + i_yPules.ToString());
                    int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                    i_d0 = i_xPules;
                    i_d2 = i_d0 / 65536;
                    if (i_d0 < 0) //负数脉冲
                    {
                        if (i_d2 == 0)
                        {
                            i_d2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_d0) > 65536)
                            {
                                i_d2 = i_d2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_d2 = i_d0 / 65536;
                    }
                    i_d1 = i_d0 % 65536;

                    int i_yd0 = 0, i_yd1 = 0, i_yd2 = 0;
                    i_yd0 = i_yPules;
                    i_yd2 = i_yd0 / 65536;
                    if (i_yd0 < 0) //负数脉冲
                    {
                        if (i_yd2 == 0)
                        {
                            i_yd2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_yd0) > 65536)
                            {
                                i_yd2 = i_yd2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_yd2 = i_yd0 / 65536;
                    }
                    i_yd1 = i_yd0 % 65536;
                    int[] ia_array = { 2, i_d1, i_d2, i_yd1, i_yd2, 0, 0, 0, 0, i_type, 1 };
                    Lib_Log.Log.writeLogException("发送坐标");
                    Console.WriteLine("发送坐标");
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入定点移动编号返回失败,继续写入");
                        Console.WriteLine("写入定点移动编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("6");
                    }
                    if (i_move_Type == 3)
                    {
                        //失能关闭
                        Power(2);
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("4") || e.Message.Equals("5") || e.Message.Equals("-2") || e.Message.Equals("6"))
                    {
                        throw e;
                    }
                    else
                    {
                        MessageBox.Show(e.ToString());
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }



        /// <summary>
        /// 抽液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="b_isTrue">排空动作</param>
        /// <param name="isBM">大小针筒 0小针筒 大针筒</param>
        /// <returns></returns>
        public static int Extract(int i_extract_Pulse, bool b_isTrue, int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                int iMRes = 0;
                Lib_Card.ADT8940A1.Module.Extraction.Extraction extraction = new Lib_Card.ADT8940A1.Module.Extraction.Extraction_Drip();
                if (b_isTrue)
                {
                    iMRes = extraction.FluidExtraction(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType, i_extract_Pulse, 1);
                }
                else
                {
                    iMRes = extraction.FluidExtraction(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType, i_extract_Pulse, 0);
                }
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {

                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }

                    Lib_Log.Log.writeLogException("执行抽液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    Console.WriteLine("开始抽液Extract_Pulse=" + i_extract_Pulse.ToString());
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Console.WriteLine("清除完标志位");
                    Lib_Log.Log.writeLogException("清除标志位结束");



                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }
                    i_extract_Pulse = i_extract_Pulse % 65536;
                    int[] ia_array = { 3, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1, 0, b_isTrue ? 1 : 0, i_syringeType };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入抽液编号返回失败,继续写入");
                        Console.WriteLine("写入抽液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("7");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("3") || e.Message.Equals("7") || e.Message.Equals("-2") || e.Message.Equals("未发现针筒"))
                    {
                        string msg = "";
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 2101)
                            {
                                throw new Exception("未发现针筒");
                            }
                            else if (ia_errArray[i] == 2102)
                            {
                                throw new Exception("取针筒时抓手关失败");
                            }
                            else if (ia_errArray[i] == 2103)
                            {
                                throw new Exception("取针筒时气缸下失败");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 混合液抽液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="b_isTrue">排空动作</param>
        /// <param name="isBM">大小针筒 0小针筒 大针筒</param>
        /// <returns></returns>
        public static int AbsExtract(int i_extract_Pulse, bool b_isTrue, int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                int iMRes = 0;
                Lib_Card.ADT8940A1.Module.Extraction.Extraction extraction = new Lib_Card.ADT8940A1.Module.Extraction.Extraction_Drip();
                if (b_isTrue)
                {
                    iMRes = extraction.FluidExtraction(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType, i_extract_Pulse, 1);
                }
                else
                {
                    iMRes = extraction.FluidExtraction(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType, i_extract_Pulse, 0);
                }
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {

                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }

                    Lib_Log.Log.writeLogException("执行抽液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    Console.WriteLine("开始抽液Extract_Pulse=" + i_extract_Pulse.ToString());
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Console.WriteLine("清除完标志位");
                    Lib_Log.Log.writeLogException("清除标志位结束");



                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }
                    i_extract_Pulse = i_extract_Pulse % 65536;
                    int[] ia_array = { 19, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1, 0, b_isTrue ? 1 : 0, i_syringeType };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入抽液编号返回失败,继续写入");
                        Console.WriteLine("写入抽液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("7");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("3") || e.Message.Equals("7") || e.Message.Equals("-2") || e.Message.Equals("未发现针筒"))
                    {
                        string msg = "";
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 2101)
                            {
                                throw new Exception("未发现针筒");
                            }
                            else if (ia_errArray[i] == 2102)
                            {
                                throw new Exception("取针筒时抓手关失败");
                            }
                            else if (ia_errArray[i] == 2103)
                            {
                                throw new Exception("取针筒时气缸下失败");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 注液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="i_syringeType">大小针筒 0 小针筒 1 大针筒</param>
        /// <returns></returns>
        public static int Shove(int i_extract_Pulse, int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion;
                infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
                int iMRes = infusion.LiquidInfusion(i_syringeType, i_extract_Pulse);
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行注液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }

                    int[] ia_array = { 4, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入注液编号返回失败,继续写入");
                        Console.WriteLine("写入注液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("8");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("8"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 注液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="i_syringeType">大小针筒 0 小针筒 1 大针筒</param>
        /// <returns></returns>
        public static int DyeShove(int i_extract_Pulse, int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //气缸到阻挡位
                CylinderMo cylinderMo = new CylinderMo();
                if (-1 == cylinderMo.CylinderBlock(1))
                    return -1;
                Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion;
                infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
                int iMRes = infusion.LiquidInfusion(i_syringeType, i_extract_Pulse);
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行注液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }

                    int[] ia_array = { 4, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入注液编号返回失败,继续写入");
                        Console.WriteLine("写入注液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("8");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("8"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 吸光度注液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="i_syringeType">大小针筒 0 小针筒 1 大针筒</param>
        /// <returns></returns>
        public static int AbsShove(int i_extract_Pulse,int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion;
                infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
                int iMRes = infusion.LiquidInfusion(i_syringeType, i_extract_Pulse);
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行注液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }

                    int[] ia_array = { 4, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入Abs注液编号返回失败,继续写入");
                        Console.WriteLine("写入Abs注液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("8");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("8"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 开稀注液
        /// </summary>
        /// <param name="i_extract_Pulse">脉冲数</param>
        /// <param name="i_syringeType">大小针筒 0 小针筒 1 大针筒</param>
        /// <returns></returns>
        public static int BrewShove(int i_extract_Pulse, int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.Infusion.Infusion infusion;
                infusion = new Lib_Card.ADT8940A1.Module.Infusion.Infusion_Up();
                int iMRes = infusion.LiquidInfusion(i_syringeType, i_extract_Pulse);
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行注液方法Extract_Pulse=" + i_extract_Pulse.ToString());

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    d_1 = i_extract_Pulse / 65536;
                    if (i_extract_Pulse < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_extract_Pulse) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_extract_Pulse / 65536;
                    }

                    int[] ia_array = { 4, 0, 0, 0, 0, i_extract_Pulse, d_1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入开稀注液编号返回失败,继续写入");
                        Console.WriteLine("写入开稀注液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("8");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("8"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        //针对绿维电机放针  还是不防针 防止异常情况 比如 在移动向天平的时候 异常了 这时候不能放针
        public static void lvPut() {

           /* int iSyringe = CardObject.OA1Input.InPutStatus(ADT8940A1_IO.InPut_Syringe);
            if (Lib_Card.Configure.Parameter.Machine_isSyringe == 1)
            {
                iSyringe = 1;
            }
            if (iSyringe == 1)
            {//在判断下针检感应器 
                DataTable dt_bottle_details = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT SyringeType FROM bottle_details WHERE BottleNum = '" + FADM_Object.Communal._i_optBottleNum + "';");
                //加个放针
                if ("小针筒" == Convert.ToString(dt_bottle_details.Rows[0][0]) || "Little Syringe" == Convert.ToString(dt_bottle_details.Rows[0][0]))
                {
                    MyModbusFun.Put(0);
                }
                else
                {
                    MyModbusFun.Put(1);
                }
            }*/

            Home.Home_XYZFinish = false; //放完针 可能推动 所以下次运动重新回原点
        }

        /// <summary>
        /// 天平检查
        /// </summary>
        /// <returns></returns>
        public static int CheckBalance()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
            label4:
                if (6666 == FADM_Object.Communal.dBalanceValue)
                {
                    ////不等于0 说明是绿维电机 并且夹着针 放针
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1 && FADM_Object.Communal._i_optBottleNum!=0)
                    //{
                    //    lvPut();
                    //}


                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                    xpower.X_Power_Off();
                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                    ypower.Y_Power_Off();
                    new FADM_Object.MyAlarm("天平通讯异常,检查恢复后请点是", "滴液", false, 1);
                    xpower.X_Power_On();
                    ypower.Y_Power_On();

                    goto label4;
                }
                else if (7777 == FADM_Object.Communal.dBalanceValue)
                {
                    ////不等于0 说明是绿维电机 并且夹着针 放针
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                    //{
                    //    lvPut();
                    //}

                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                    xpower.X_Power_Off();
                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                    ypower.Y_Power_Off();
                    new FADM_Object.MyAlarm("开机未拿走废液桶,请先拿走废液桶，等待天平清零后重新放置废液桶，然后点确定", "滴液", false, 1);
                    xpower.X_Power_On();
                    ypower.Y_Power_On();
                    goto label4;
                }
                else if (8888 == FADM_Object.Communal.dBalanceValue)
                {
                    ////不等于0 说明是绿维电机 并且夹着针 放针
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                    //{
                    //    lvPut();
                    //}

                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                    xpower.X_Power_Off();
                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                    ypower.Y_Power_Off();
                    new FADM_Object.MyAlarm("天平超下限,检查恢复后请点是", "滴液", false, 1);
                    xpower.X_Power_On();
                    ypower.Y_Power_On();
                    goto label4;
                }
                else if (9999 == FADM_Object.Communal.dBalanceValue)
                {
                    ////不等于0 说明是绿维电机 并且夹着针 放针
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                    //{
                    //    lvPut();
                    //}

                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                    xpower.X_Power_Off();
                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                    ypower.Y_Power_Off();
                    new FADM_Object.MyAlarm("天平超上限,检查恢复后请点是", "滴液", false, 1);
                    xpower.X_Power_On();
                    ypower.Y_Power_On();
                    goto label4;
                }
                else if (FADM_Object.Communal._b_balanceAlarm)
                {
                    ////不等于0 说明是绿维电机 并且夹着针 放针
                    //if (Lib_Card.Configure.Parameter.Machine_Type == 0 && Lib_Card.Configure.Parameter.Machine_Type_Lv == 1)
                    //{
                    //    lvPut();
                    //}

                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power xpower = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Basic();
                    xpower.X_Power_Off();
                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power ypower = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Basic();
                    ypower.Y_Power_Off();
                    new FADM_Object.MyAlarm("请先清空废液桶，然后点确定?", "滴液", false, 1);
                    xpower.X_Power_On();
                    ypower.Y_Power_On();
                    goto label4;
                }

                return 0;
            }
            else
            {
                try
                {
                    //Label1:
                    //    if (!FADM_Object.Communal._b_auto)
                    //    {//在手动页面 等待手动页面退出
                    //        goto Label1;
                    //    }
                    Lib_Log.Log.writeLogException("执行天平检查CheckBalance");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:


                    int[] ia_array = { 13 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入天平检查编号返回失败,继续写入");
                        Console.WriteLine("写入天平检查编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("8");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("8"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 加水
        /// </summary>
        /// <param name="d_addWaterTime">加水时间</param>
        /// <returns></returns>
        public static int AddWater(double d_addWaterTime)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                new Water().Add(d_addWaterTime, "RobotHand");
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行加水方法addWaterTime = " + d_addWaterTime.ToString());
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");


                lableTop:
                    int[] ia_array = { 5, 0, 0, 0, 0, 0, 0, Lib_Card.Configure.Parameter.Other_WaterMagnification == 0 ? (int)(d_addWaterTime * 1000) : (int)(d_addWaterTime * 100), 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入加水编号返回失败,继续写入");
                        Console.WriteLine("写入加水编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("9");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("9"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 加溶解剂
        /// </summary>
        /// <param name="d_addWaterTime">加加溶解剂时间</param>
        /// <returns></returns>
        public static int AddDissolving(double d_addWaterTime)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //OpenOrCloseBear();
                //new Water().Add(d_addWaterTime, "RobotHand");
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行加加溶解剂方法addTime = " + d_addWaterTime.ToString());
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");


                lableTop:
                    int[] ia_array = { 22, 0, 0, 0, 0, 0, 0, Lib_Card.Configure.Parameter.Other_WaterMagnification == 0 ? (int)(d_addWaterTime * 1000) : (int)(d_addWaterTime * 100), 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入加加溶解剂编号返回失败,继续写入");
                        Console.WriteLine("写入加加溶解剂编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("9");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("9"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 计算加溶解剂时间
        /// </summary>
        /// <returns></returns>
        public static double GetDissolvingTime(double d_water)
        {
            //计算加溶解剂时间
            double d_blTime = 0;


            d_blTime = d_water / Lib_Card.Configure.Parameter.Correcting_Dissolving_Value;


            return d_blTime;
        }


        /// <summary>
        /// 泄压
        /// </summary>
        /// <param name="i_xStartPules">开始X坐标</param>
        /// <param name="i_yStartPules">开始Y坐标</param>
        /// <param name="i_cupNum">杯号</param>
        /// <returns></returns>
        public static int Stressrelief(int i_xStartPules, int i_yStartPules,int i_cupNum)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                try
                {

                    //等待机械手状态
                    while (true)
                    {
                        //判断当前机台状态
                        //滴液过程，可抢占线程
                        if (7 == FADM_Object.Communal.ReadMachineStatus())
                        {
                            if (FADM_Object.Communal.ReadDripWait())
                                break;
                        }
                        //待机状态下直接进入
                        else if (0 == FADM_Object.Communal.ReadMachineStatus())
                            break;
                        //其他状态下直接等待完成后再进行


                        Thread.Sleep(1);
                    }
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNum + "号配液杯泄压启动");

                    //寻找泄压位
                    //Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.Move_Decompression();
                    //int iMRes = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_cupNum);
                    //if (-1 == iMRes)
                    //    throw new Exception("驱动异常");
                    //else if (-2 == iMRes)
                    //    throw new Exception("收到退出消息");

                    int i_mRes1 = MyModbusFun.TargetMove(5, i_cupNum, 0);
                    if (-2 == i_mRes1)
                        throw new Exception("收到退出消息");

                    //伸出接液盘
                    Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Condition();
                    if (-1 == tray.Tray_On())
                        throw new Exception("驱动异常");
                    if (Lib_Card.Configure.Parameter.Machine_Decompression == 1)
                    {
                        //泄压下
                        Lib_Card.ADT8940A1.OutPut.Decompression.Decompression decompression = new Lib_Card.ADT8940A1.OutPut.Decompression.Decompression_Condition();
                        if (-1 == decompression.Decompression_Down())
                            throw new Exception("驱动异常");

                        //泄压
                        Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_DecoTime * 1000.00));

                        //泄压上
                        if (-1 == decompression.Decompression_Up())
                            throw new Exception("驱动异常");
                    }
                    else
                    {
                        //区域4有布局时
                        if (Lib_Card.Configure.Parameter.Machine_Area4_Type != 0)
                        {
                            //使用原来的
                            if (i_cupNum >= Lib_Card.Configure.Parameter.Machine_Area4_CupMin)
                            {
                                //泄压下
                                Lib_Card.ADT8940A1.OutPut.Decompression.Decompression decompression = new Lib_Card.ADT8940A1.OutPut.Decompression.Decompression_Condition();
                                if (-1 == decompression.Decompression_Down())
                                    throw new Exception("驱动异常");

                                //泄压
                                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_DecoTime * 1000.00));

                                //泄压上
                                if (-1 == decompression.Decompression_Up())
                                    throw new Exception("驱动异常");
                            }
                            //使用新增泄压位
                            else
                            {
                                //泄压下
                                Lib_Card.ADT8940A1.OutPut.Decompression.Decompression decompression = new Lib_Card.ADT8940A1.OutPut.Decompression.Decompression_Condition();
                                if (-1 == decompression.Decompression_Down_Right())
                                    throw new Exception("驱动异常");

                                //泄压
                                Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_DecoTime * 1000.00));

                                //泄压上
                                if (-1 == decompression.Decompression_Up_Right())
                                    throw new Exception("驱动异常");
                            }
                        }
                        //区域4没布局，直接用原有的
                        else
                        {
                            //泄压下
                            Lib_Card.ADT8940A1.OutPut.Decompression.Decompression decompression = new Lib_Card.ADT8940A1.OutPut.Decompression.Decompression_Condition();
                            if (-1 == decompression.Decompression_Down())
                                throw new Exception("驱动异常");

                            //泄压
                            Thread.Sleep(Convert.ToInt32(Lib_Card.Configure.Parameter.Delay_DecoTime * 1000.00));

                            //泄压上
                            if (-1 == decompression.Decompression_Up())
                                throw new Exception("驱动异常");
                        }
                    }

                    

                    //复位占用标志位
                    FADM_Object.Communal._fadmSqlserver.InsertRun("Dail", i_cupNum + "号配液杯泄压完成");

                    

                }
                catch (Exception ex)
                {

                    if ("收到退出消息" == ex.Message)
                    {
                        FADM_Object.Communal._b_stop = false;
                        new Reset().MachineReset();
                    }

                    else
                    {
                        FADM_Object.Communal.WriteMachineStatus(8);
                        FADM_Form.CustomMessageBox.Show(ex.Message, "Stressrelief", MessageBoxButtons.OK, true);
                    }
                }

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }

                    Lib_Log.Log.writeLogException("执行泄压方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int[] ia_array = { 7, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入泄压动作编号返回失败,继续写入");
                        Console.WriteLine("泄压编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 放针
        /// </summary>
        /// <param name="i_syringeType">大小针筒 0 小针筒 1 大针筒</param>
        /// <returns></returns>
        public static int Put(int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                int iMRes = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType);
                if (-1 == iMRes)
                    throw new Exception("驱动异常");
                else if (-2 == iMRes)
                    throw new Exception("收到退出消息");
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行放针方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int[] ia_array = { 6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入放针动作编号返回失败,继续写入");
                        Console.WriteLine("放针编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }


        /// <summary>
        /// 放盖(关盖)
        /// </summary>
        /// <returns></returns>
        public static int PutCover()
        {
            try
            {
            Label1:
                if (!FADM_Object.Communal._b_auto)
                {//在手动页面 等待手动页面退出
                    goto Label1;
                }
                Lib_Log.Log.writeLogException("执行放盖方法");

                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                ClearSuccessState();//先清除标志位
                Lib_Log.Log.writeLogException("清除标志位结束");

            lableTop:
                int[] ia_array = { 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                if (i_state != -1)
                {
                    //判断错误返回值
                    if (GetReturn(1) == -2)
                    {
                        return -2;
                    }
                    else
                    {

                    }
                }
                else
                {
                    Lib_Log.Log.writeLogException("写入放盖动作编号返回失败,继续写入");
                    Console.WriteLine("放盖编号返回失败,继续写入");
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto lableTop;
                    }
                    b_istrue = true;
                    goto lableTop;
                    //throw new Exception("10");
                }
                return 0;
            }
            catch (Exception e)
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
                if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                {
                    throw e;
                }
                else
                {
                    throw new Exception("-1");
                }
            }
            finally
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
            }
        }

        /// <summary>
        /// 开关盖
        /// </summary>
        /// <param name="i_xStartPules">开始X坐标</param>
        /// <param name="i_yStartPules">开始Y坐标</param>
        /// <param name="i_xEndPules">结束X坐标</param>
        /// <param name="i_yEndPules">结束Y坐标</param>
        /// <param name="i_type">0:开盖 1：关盖 2:测试坐标</param>
        /// <returns></returns>
        public static int OpenOrPutCover(int i_xStartPules, int i_yStartPules, int i_xEndPules, int i_yEndPules, int i_type)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                int iRes = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                if (1 == iRes)
                {
                    Thread.Sleep(1000);
                    iRes = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                    if (1 == iRes)
                        throw new Exception("发现杯盖或针筒");
                }
                //移动到拿盖位置
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xStartPules, i_yStartPules, 0);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                if (Lib_Card.Configure.Parameter.Machine_CloseCoverType == 0)
                {
                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power x_Power = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Condition();
                    if (-1 == x_Power.X_Power_Off())
                        return -1;

                    //Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power y_Power = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Condition();
                    //if (-1 == y_Power.Y_Power_Off())
                    //    return -1;
                }
                try
                {
                    Lib_Card.ADT8940A1.Module.GetOrPutCover getOrPutCover = new Lib_Card.ADT8940A1.Module.GetOrPutCover();
                    getOrPutCover.GetCover(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_type);
                }
                catch (Exception ex)
                {
                    if ("收到退出消息" == ex.Message)
                    {
                        FADM_Object.Communal._b_stop = false;
                        new Reset().MachineReset();
                    }
                    else
                    {
                        throw ex;
                    }
                }
                if (i_type != 2)
                {
                    iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xEndPules, i_yEndPules, 0);
                    if (-1 == iMove)
                        throw new Exception("驱动异常");
                    else if (-2 == iMove)
                        throw new Exception("收到退出消息");
                }


                //Lib_Card.ADT8940A1.OutPut.X_Power.X_Power x_Power = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Condition();
                //if (-1 == x_Power.X_Power_Off())
                //    return -1;

                //Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power y_Power = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Condition();
                //if (-1 == y_Power.Y_Power_Off())
                //    return -1;

                try
                {
                    Lib_Card.ADT8940A1.Module.GetOrPutCover getOrPutCover = new Lib_Card.ADT8940A1.Module.GetOrPutCover();
                    getOrPutCover.PutCover(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_type, i_xEndPules, i_yEndPules+ Lib_Card.Configure.Parameter.Other_SupportCoverY);
                }
                catch (Exception ex)
                {
                    if ("收到退出消息" == ex.Message)
                    {
                        FADM_Object.Communal._b_stop = false;
                        new Reset().MachineReset();
                    }
                    else
                    {
                        throw ex;
                    }
                }


                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {
                        if(i_type !=2)
                        //在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行放关盖方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int d_3 = 0;
                    if (i_xEndPules > 65536)
                    {
                        d_3 = i_xEndPules / 65536;
                        i_xEndPules = i_xEndPules % 65536;
                    }
                    int d_4 = 0;
                    if (i_yEndPules > 65536)
                    {
                        d_4 = i_yEndPules / 65536;
                        i_yEndPules = i_yEndPules % 65536;
                    }
                    int[] ia_array = { 11, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, i_xEndPules, d_3, i_yEndPules, d_4, 0, 0, i_type
                    ,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1,true) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入开关盖动作编号返回失败,继续写入");
                        Console.WriteLine("开关盖编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 2702)
                            {
                                throw new Exception("未发现杯盖");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }
                            else if (ia_errArray[i] == 2703)
                            {
                                throw new Exception("配液杯取盖失败");
                            }
                            else if (ia_errArray[i] == 2704)
                            {
                                throw new Exception("放盖区取盖失败");
                            }
                            else if (ia_errArray[i] == 2705)
                            {
                                throw new Exception("关盖失败");
                            }
                            else if (ia_errArray[i] == 2706)
                            {
                                throw new Exception("放盖失败");
                            }
                            else if (ia_errArray[i] == 2707)
                            {
                                throw new Exception("二次关盖复压失败");
                            }
                            else if (ia_errArray[i] == 2708)
                            {
                                throw new Exception("二次关盖失败");
                            }
                            else if (ia_errArray[i] == 2709)
                            {
                                throw new Exception("二次关盖未发现杯盖");
                            }
                            else if (ia_errArray[i] == 2710)
                            {
                                throw new Exception("移动前抓手打开异常");
                            }
                            else if (ia_errArray[i] == 2711)
                            {
                                throw new Exception("移动前抓手打开关闭都发现异常");
                            }
                            else if (ia_errArray[i] == 2712)
                            {
                                throw new Exception("感应杯盖时打开抓手失败");
                            }
                            else if (ia_errArray[i] == 2713)
                            {
                                throw new Exception("配液杯取盖失败抓手关失败");
                            }
                            else if (ia_errArray[i] == 2714)
                            {
                                throw new Exception("放盖区取盖失败抓手关失败");
                            }
                            else if (ia_errArray[i] == 2715)
                            {
                                throw new Exception("放盖后抓手打开失败");
                            }
                            else if (ia_errArray[i] == 2716)
                            {
                                throw new Exception("关盖时抓手打开失败");
                            }
                            else if (ia_errArray[i] == 2717)
                            {
                                throw new Exception("关盖时撑盖开失败");
                            }
                            else if (ia_errArray[i] == 2718)
                            {
                                throw new Exception("关盖撑盖时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2719)
                            {
                                throw new Exception("开盖前染杯无上锁止");
                            }
                            else if (ia_errArray[i] == 2720)
                            {
                                throw new Exception("关盖前染杯无上锁止");
                            }
                            else if (ia_errArray[i] == 2730)
                            {
                                throw new Exception("关盖时染杯无上锁止");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 出布/放布
        /// </summary>
        /// <param name="i_xStartPules">开始X坐标</param>
        /// <param name="i_yStartPules">开始Y坐标</param>
        /// <param name="i_xEndPules">结束X坐标</param>
        /// <param name="i_yEndPules">结束Y坐标</param>
        /// <param name="i_getCloth">0:备布区 1：缸体</param>
        /// <param name="i_putCloth">0:完成区 1：缸体</param>
        /// <param name="d_clothWeight">布重</param>
        /// <returns></returns>
        public static int PutOrGetCloth(int i_xStartPules, int i_yStartPules, int i_xEndPules, int i_yEndPules, int i_getCloth, int i_putCloth, double d_clothWeight)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.GetOrPutCloth getOrPutCloth = new Lib_Card.ADT8940A1.Module.GetOrPutCloth();
                //放布
                if (i_getCloth == 0)
                {
                    getOrPutCloth.GetOrPut(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 1, i_xStartPules, i_yStartPules, i_xEndPules, i_yEndPules);
                }
                //出布
                else
                {
                    getOrPutCloth.GetOrPut(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 2, i_xStartPules, i_yStartPules, i_xEndPules, i_yEndPules);
                }
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行出布/放布方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int d_3 = 0;
                    if (i_xEndPules > 65536)
                    {
                        d_3 = i_xEndPules / 65536;
                        i_xEndPules = i_xEndPules % 65536;
                    }
                    int d_4 = 0;
                    if (i_yEndPules > 65536)
                    {
                        d_4 = i_yEndPules / 65536;
                        i_yEndPules = i_yEndPules % 65536;
                    }
                    int[] ia_array = { 16, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                        i_xEndPules, d_3, i_yEndPules, d_4, 0, 0, 0, 0, 0, i_getCloth, i_putCloth, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (int)(d_clothWeight*100) };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入出布/放布动作编号返回失败,继续写入");
                        Console.WriteLine("出布/放布编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        //    string msg = "";
                        //    int[] ia_errArray = new int[100];
                        //Label123:
                        //    int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        //    if (i_state1 == -1)
                        //        goto Label123;
                        //    for (int i = 0; i < ia_errArray.Length; i++)
                        //    {
                        //        if (ia_errArray[i] == 2702)
                        //        {
                        //            throw new Exception("未发现杯盖");
                        //        }
                        //        else if (ia_errArray[i] == 2701)
                        //        {
                        //            throw new Exception("发现杯盖或针筒");
                        //        }

                        //    }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }


        /// <summary>
        /// 出布/放布(转子)
        /// </summary>
        /// <param name="i_xStartPules">开始X坐标</param>
        /// <param name="i_yStartPules">开始Y坐标</param>
        /// <param name="i_xEndPules">结束X坐标</param>
        /// <param name="i_yEndPules">结束Y坐标</param>
        /// <param name="i_xt">挡水板X坐标</param>
        /// <param name="i_yt">挡水板Y坐标</param>
        /// <param name="i_getCloth">0:备布区 1：缸体</param>
        /// <param name="i_putCloth">0:完成区 1：缸体</param>
        /// <returns></returns>
        public static int PutOrGetCloth_Rotor(int i_xStartPules, int i_yStartPules, int i_xEndPules, int i_yEndPules, int i_getCloth, int i_putCloth, int i_xt, int i_yt)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                Lib_Card.ADT8940A1.Module.GetOrPutCloth getOrPutCloth = new Lib_Card.ADT8940A1.Module.GetOrPutCloth();
                //放布
                if (i_getCloth == 0)
                {
                     return getOrPutCloth.GetOrPut_Rotor(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 1, i_xStartPules, i_yStartPules, i_xEndPules, i_yEndPules, i_xt, i_yt);
                }
                //出布
                else
                {
                    return getOrPutCloth.GetOrPut_Rotor(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 2, i_xStartPules, i_yStartPules, i_xEndPules, i_yEndPules, i_xt, i_yt);
                }
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行出布/放布方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int d_3 = 0;
                    if (i_xEndPules > 65536)
                    {
                        d_3 = i_xEndPules / 65536;
                        i_xEndPules = i_xEndPules % 65536;
                    }
                    int d_4 = 0;
                    if (i_yEndPules > 65536)
                    {
                        d_4 = i_yEndPules / 65536;
                        i_yEndPules = i_yEndPules % 65536;
                    }
                    int[] ia_array = { 16, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, i_xEndPules, d_3, i_yEndPules, d_4, 0, 0, 0, 0, 0, i_getCloth, i_putCloth };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入出布/放布动作编号返回失败,继续写入");
                        Console.WriteLine("出布/放布编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        //    string msg = "";
                        //    int[] ia_errArray = new int[100];
                        //Label123:
                        //    int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        //    if (i_state1 == -1)
                        //        goto Label123;
                        //    for (int i = 0; i < ia_errArray.Length; i++)
                        //    {
                        //        if (ia_errArray[i] == 2702)
                        //        {
                        //            throw new Exception("未发现杯盖");
                        //        }
                        //        else if (ia_errArray[i] == 2701)
                        //        {
                        //            throw new Exception("发现杯盖或针筒");
                        //        }

                        //    }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 失能开关
        /// </summary>
        /// <returns></returns>
        public static int Power(int i_type)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                if (i_type == 2)
                {
                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power x_Power = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Condition();
                    if (-1 == x_Power.X_Power_Off())
                        return -1;

                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power y_Power = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Condition();
                    if (-1 == y_Power.Y_Power_Off())
                        return -1;
                }
                else if (i_type == 1)
                {
                    Lib_Card.ADT8940A1.OutPut.X_Power.X_Power x_Power = new Lib_Card.ADT8940A1.OutPut.X_Power.X_Power_Condition();
                    if (-1 == x_Power.X_Power_On())
                        return -1;

                    Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power y_Power = new Lib_Card.ADT8940A1.OutPut.Y_Power.Y_Power_Condition();
                    if (-1 == y_Power.Y_Power_On())
                        return -1;
                }
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("失能" + i_type);

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询

                lableTop:
                    int[] ia_array = new int[1];
                    ia_array[0] = i_type;
                    int i_state = FADM_Object.Communal._tcpModBus.Write(808, ia_array);
                    if (i_state != -1)
                    {
                        //写入成功  
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("失能" + i_type + "写入失败");
                        Console.WriteLine("失能" + i_type + "写入失败继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 开盖
        /// </summary>
        /// <returns></returns>
        public static int OpenCover()
        {
            try
            {
            Label1:
                if (!FADM_Object.Communal._b_auto)
                {//在手动页面 等待手动页面退出
                    goto Label1;
                }
                Lib_Log.Log.writeLogException("执行开盖方法");

                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                ClearSuccessState();//先清除标志位
                Lib_Log.Log.writeLogException("清除标志位结束");

            lableTop:
                int[] ia_array = { 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                if (i_state != -1)
                {
                    //判断错误返回值
                    if (GetReturn(1) == -2)
                    {
                        return -2;
                    }
                    else
                    {

                    }
                }
                else
                {
                    Lib_Log.Log.writeLogException("写入开盖动作编号返回失败,继续写入");
                    Console.WriteLine("开盖编号返回失败,继续写入");
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto lableTop;
                    }
                    b_istrue = true;
                    goto lableTop;
                    //throw new Exception("10");
                }
                return 0;
            }
            catch (Exception e)
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
                if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                {
                    int[] ia_errArray = new int[100];
                Label123:
                    int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                    if (i_state1 == -1)
                        goto Label123;
                    for (int i = 0; i < ia_errArray.Length; i++)
                    {
                        if (ia_errArray[i] == 2702)
                        {
                            throw new Exception("未发现杯盖");
                        }
                        else if (ia_errArray[i] == 2701)
                        {
                            throw new Exception("发现杯盖或针筒");
                        }

                    }
                    throw e;
                }
                else
                {
                    throw new Exception("-1");
                }
            }
            finally
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
            }
        }

        /// <summary>
        /// 拿夹子 i_type 1:干布 2：湿布 3：粉筒夹 4：PH测量  5：拿粉罐
        /// </summary>
        /// <returns></returns>
        public static int GetClamp( int i_xStartPules, int i_yStartPules,int i_type)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                //移动到布夹位置
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xStartPules, i_yStartPules, 0);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                //拿布夹
                Lib_Card.ADT8940A1.Module.GetOrPutClamp getOrPutClamp=new Lib_Card.ADT8940A1.Module.GetOrPutClamp();
                getOrPutClamp.GetClamp(Lib_Card.Configure.Parameter.Machine_CylinderVersion);
                if (i_type == 1)
                {
                    FADM_Object.Communal._b_isGetDryClamp = true;
                    FADM_Object.Communal._b_isGetWetClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = false;
                    FADM_Object.Communal._b_isGetPHClamp = false;
                }
                else if (i_type == 2)
                {
                    FADM_Object.Communal._b_isGetWetClamp = true;
                    FADM_Object.Communal._b_isGetDryClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = false;
                    FADM_Object.Communal._b_isGetPHClamp = false;
                }
                else if (i_type == 3)
                {
                    FADM_Object.Communal._b_isGetWetClamp = false;
                    FADM_Object.Communal._b_isGetDryClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = true;
                    FADM_Object.Communal._b_isGetPHClamp = false;
                }
                else if (i_type == 4)
                {
                    FADM_Object.Communal._b_isGetWetClamp = false;
                    FADM_Object.Communal._b_isGetDryClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = false;
                    FADM_Object.Communal._b_isGetPHClamp = true;
                }
                else if (i_type == 5)
                {
                    FADM_Object.Communal._b_isGetWetClamp = false;
                    FADM_Object.Communal._b_isGetDryClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = false;
                    FADM_Object.Communal._b_isGetPHClamp = false;
                }
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行拿夹子方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }

                    int d_3 = 0;
                    if (i_type == 5)
                        d_3 = 2;
                    else if (i_type == 3)
                        d_3 = 3;

                    int[] ia_array = { 14, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1, 0, (i_type == 4 ? 1 : 0), d_3 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入拿夹子动作编号返回失败,继续写入");
                        Console.WriteLine("拿夹子编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                    if (i_type == 1)
                    {
                        FADM_Object.Communal._b_isGetDryClamp = true;
                        FADM_Object.Communal._b_isGetWetClamp = false;
                        FADM_Object.Communal._b_isGetPowerClamp = false;
                        FADM_Object.Communal._b_isGetPHClamp = false;
                    }
                    else if (i_type == 2)
                    {
                        FADM_Object.Communal._b_isGetWetClamp = true;
                        FADM_Object.Communal._b_isGetDryClamp = false;
                        FADM_Object.Communal._b_isGetPowerClamp = false;
                        FADM_Object.Communal._b_isGetPHClamp = false;
                    }
                    else if (i_type == 3)
                    {
                        FADM_Object.Communal._b_isGetWetClamp = false;
                        FADM_Object.Communal._b_isGetDryClamp = false;
                        FADM_Object.Communal._b_isGetPowerClamp = true;
                        FADM_Object.Communal._b_isGetPHClamp = false;
                    }
                    else if (i_type == 4)
                    {
                        FADM_Object.Communal._b_isGetWetClamp = false;
                        FADM_Object.Communal._b_isGetDryClamp = false;
                        FADM_Object.Communal._b_isGetPowerClamp = false;
                        FADM_Object.Communal._b_isGetPHClamp = true;
                    }
                    else if (i_type == 5)
                    {
                        FADM_Object.Communal._b_isGetWetClamp = false;
                        FADM_Object.Communal._b_isGetDryClamp = false;
                        FADM_Object.Communal._b_isGetPowerClamp = false;
                        FADM_Object.Communal._b_isGetPHClamp = false;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现抓手");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 4504)
                            {
                                throw new Exception("称粉电机回原点失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 拿针筒，用于ABS抽混合液 
        /// </summary>
        /// <returns></returns>
        public static int GetSyringes(int i_xStartPules, int i_yStartPules)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行拿针筒方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }

                    int[] ia_array = { 14, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1,0,0 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入拿针筒动作编号返回失败,继续写入");
                        Console.WriteLine("拿针筒编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                    FADM_Object.Communal._b_isGetSyringes = true;
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现抓手");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 放夹子 1:干布夹子 2:湿布夹子
        /// </summary>
        /// <returns></returns>
        public static int PutClamp(int i_xStartPules, int i_yStartPules)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                //移动到布夹位置
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xStartPules, i_yStartPules, 0);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                //拿布夹
                Lib_Card.ADT8940A1.Module.GetOrPutClamp getOrPutClamp = new Lib_Card.ADT8940A1.Module.GetOrPutClamp();
                getOrPutClamp.PutClamp(Lib_Card.Configure.Parameter.Machine_CylinderVersion);
                //把抓夹子状态置为false
                FADM_Object.Communal._b_isGetDryClamp = false;
                FADM_Object.Communal._b_isGetWetClamp = false;
                FADM_Object.Communal._b_isGetPowerClamp = false;
                FADM_Object.Communal._b_isGetPHClamp = false;
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行放盖方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int[] ia_array = { 15, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1,0,0, FADM_Object.Communal._b_isGetPowerClamp?3:0 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入放盖动作编号返回失败,继续写入");
                        Console.WriteLine("放盖编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    //把抓夹子状态置为false
                    FADM_Object.Communal._b_isGetDryClamp = false;
                    FADM_Object.Communal._b_isGetWetClamp = false;
                    FADM_Object.Communal._b_isGetPowerClamp = false;
                    FADM_Object.Communal._b_isGetPHClamp = false;
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }


        /// <summary>
        /// 读取异常信息地址
        /// </summary>
        /// <returns>返回异常编号和信息</returns>
        public static int GetErrMsg(ref string[] sa_rarray)
        {
            try
            {
                Lib_Log.Log.writeLogException("读取异常信息地址");
                Console.WriteLine("读取异常信息地址");
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
            lableTop:
                int[] ia_array = { 0 };
                int i_state = FADM_Object.Communal._tcpModBus.Read(900, 1, ref ia_array);
                if (i_state != -1)
                {
                    Console.WriteLine("异常是" + ia_array[0].ToString());
                    Lib_Log.Log.writeLogException("异常是" + ia_array[0].ToString());
                    if (ia_array[0] != 0 && SmartDyeing.FADM_Object.Communal._dic_errModbusNo.ContainsKey(ia_array[0]))
                    {
                        string s_message = SmartDyeing.FADM_Object.Communal._dic_errModbusNo[ia_array[0]];
                        sa_rarray[0] = ia_array[0].ToString();
                        sa_rarray[1] = s_message;
                    }
                    else
                    {
                        Console.WriteLine("未知异常");
                        Lib_Log.Log.writeLogException("未知异常");

                        sa_rarray[0] = "0";
                        sa_rarray[1] = "未知异常";
                    }

                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto lableTop;
                    }
                    b_istrue = true;
                    goto lableTop;
                }
                Lib_Log.Log.writeLogException("读取异常信息地址 结束");
                return i_state;
            }
            catch (Exception e)
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
                return -1;
            }
            finally
            {
                FADM_Object.Communal.WriteTcpStatus(true); //恢复
            }
        }

        /// <summary>
        /// 读取异常信息地址
        /// </summary>
        /// <returns>返回异常编号和信息</returns>
        public static int GetErrMsgNew(ref int[] ia_array)
        {
            try
            {
                Lib_Log.Log.writeLogException("读取异常信息地址");
                Console.WriteLine("读取异常信息地址");
                bool b_istrue = false;
            //FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询

            lableTop:
                int i_state = FADM_Object.Communal._tcpModBus.Read(3000, 100, ref ia_array);
                if (i_state != -1)
                {
                    Lib_Log.Log.writeLogException("异常读取成功");
                    //if (ia_array[0] != 0 && SmartDyeing.FADM_Object.Communal._dic_errModbusNo.ContainsKey(ia_array[0]))
                    //{
                    //    string s_message = SmartDyeing.FADM_Object.Communal._dic_errModbusNo[ia_array[0]];
                    //    sa_rarray[0] = ia_array[0].ToString();
                    //    sa_rarray[1] = s_message;
                    //}
                    //else
                    //{
                    //    Console.WriteLine("未知异常");
                    //    Lib_Log.Log.writeLogException("未知异常");

                    //    sa_rarray[0] = "0";
                    //    sa_rarray[1] = "未知异常";
                    //}

                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto lableTop;
                    }
                    b_istrue = true;
                    goto lableTop;
                }
                Lib_Log.Log.writeLogException("读取异常信息地址 结束");
                return i_state;
            }
            catch (Exception e)
            {
                //FADM_Object.Communal.WriteTcpStatus(true); //恢复
                return -1;
            }
            finally
            {
                //FADM_Object.Communal.WriteTcpStatus(true); //恢复
            }
        }

        /// <summary>
        /// 读取Z轴逻辑位置
        /// </summary>
        /// <returns></returns>
        public static int GetZPosition(ref int i_position)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)

            {
                int iZPulse = 0;

                if (-1 == Lib_Card.CardObject.OA1.ReadAxisCommandPosition(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z, ref iZPulse))
                    throw new Exception("驱动异常");
                i_position = iZPulse;
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("读取Z轴逻辑位置");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    double d_value = 0.0;

                lableTop:
                    int[] ia_array = { 0, 0 };
                    int i_state = FADM_Object.Communal._tcpModBus.Read(913, 2, ref ia_array);
                    if (i_state != -1)
                    {
                        int i_a13 = ia_array[0];
                        int i_a14 = ia_array[1];
                        if (i_a13 < 0)
                        {
                            d_value = (((i_a14 + 1) * 65536 + i_a13));
                        }
                        else
                        {
                            d_value = ((i_a14 * 65536 + i_a13));
                        }
                        Console.WriteLine("读取Z轴位置是" + d_value.ToString());
                        //i_position = d_value < 0 ? Convert.ToInt32(Math.Floor(d_value)) : Convert.ToInt32(Math.Ceiling(d_value));
                        i_position = Convert.ToInt32(d_value);
                        Console.WriteLine("读取Z轴位置是" + i_position.ToString());
                        Lib_Log.Log.writeLogException("读取Z轴位置是" + i_position.ToString());
                    }
                    else
                    {
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                    }
                    Lib_Log.Log.writeLogException("读取Z轴位置结束!");
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    return -1;
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }

        /// <summary>
        /// 计算加水时间
        /// </summary>
        /// <returns></returns>
        public static double GetWaterTime(double d_water)
        {
            //计算加水时间
            double d_blTime = 0;
            if (0 == FADM_Object.Communal._i_waterWay)
            {
                if (Lib_Card.Configure.Parameter.Correcting_Water_RWeight > Lib_Card.Configure.Parameter.Correcting_Water_FWeight)
                    d_blTime = (Lib_Card.Configure.Parameter.Correcting_Water_RWeight - Lib_Card.Configure.Parameter.Correcting_Water_FWeight) / Lib_Card.Configure.Parameter.Correcting_Water_Value + 1;
                else
                {
                    d_blTime = Lib_Card.Configure.Parameter.Correcting_Water_RWeight / Lib_Card.Configure.Parameter.Correcting_Water_Value;
                    if (d_blTime < 0.01)
                        d_blTime = 0.01;
                }
            }
            else
            {
                if (FADM_Object.Communal._b_isUseNewCorrectingWater)
                {
                    d_blTime = d_water / Lib_Card.Configure.Parameter.Correcting_Water_Value;
                }
                else
                {
                    if (d_water > 13)
                        d_blTime = ((d_water - Lib_Card.Configure.Parameter.Correcting_Water_FWeight) / Lib_Card.Configure.Parameter.Correcting_Water_Value + 1) * (1 + Lib_Card.Configure.Parameter.Other_Coefficient_Water);
                    else
                        d_blTime = d_water / Lib_Card.Configure.Parameter.Correcting_Water_Value + Lib_Card.Configure.Parameter.Other_Coefficient_Water_Low;
                }
            }
            return d_blTime;
        }

        /// <summary>
        /// 清除执行完成标志位 改为0
        /// </summary>
        public static void ClearSuccessState()
        {
            Lib_Log.Log.writeLogException("执行清除标志位");
            bool b_istrue = false;
            FADM_Object.Communal.WriteTcpStatus(false); //恢复
        //清掉执行完成 标志位
        label2:
            int[] ia_array3 = { 0 };
            int istate = FADM_Object.Communal._tcpModBus.Write(904, ia_array3);
            if (istate != -1)
            {
                //清除标志位成功 重新读一下
                istate = FADM_Object.Communal._tcpModBus.Read(904, 1, ref ia_array3);
                if (istate != -1 && ia_array3[0] == 0)
                {
                    return;
                }
                else
                {
                    FADM_Object.Communal._tcpModBus.ReConnect();
                    goto label2;
                }
            }
            else
            {
                if (b_istrue)
                {
                    b_istrue = false;
                    FADM_Object.Communal._tcpModBus.ReConnect();
                    goto label2;
                }
                b_istrue = true;
                goto label2;
            }
        }
        /// <summary>
        /// 一直读取等待标志位和光幕急停
        /// </summary>
        public static int[] AwaitSuccess()
        {
            bool b_istrue = false;
            int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0,0,0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0};
            int i_state = 0;

        Label1:

            if (Lib_Card.Configure.Parameter.Other_UseAbs == 1)
            {
                //读取吸光度数据
                int[] ia_array_Abs = new int[40];
                int i_state_Abs = FADM_Object.Communal._tcpModBusAbs.Read(13188, 40, ref ia_array_Abs);
                if (i_state_Abs != -1)
                {
                    Communal._ia_d13188 = ia_array_Abs;
                }

                if (Communal._b_IsABSOpen)
                {
                    int[] ia_array_Abs2 = new int[100];
                    int i_state_Abs2 = FADM_Object.Communal._tcpModBusAbs.Read(13088, 100, ref ia_array_Abs2);
                    if (i_state_Abs2 != -1)
                    {
                        Communal._ia_d13088 = ia_array_Abs2;
                    }
                }
            }

            if (FADM_Object.Communal._b_isHaveAutoBrew)
            {
                //读取自动开料数据
                int[] ia_array_Brew = new int[75];
                int i_state_Brew = FADM_Object.Communal._tcpModBus.Read(22688, 75, ref ia_array_Brew);
                if (i_state_Brew != -1)
                {
                    Communal._ia_d22688 = ia_array_Brew;
                }
            }
            if (FADM_Object.Communal._b_isHaveAutoBrew)
            {
                //读取自动开料数据
                int[] ia_array_WashBottle = new int[10];
                int i_state_WashBottle = FADM_Object.Communal._tcpModBus.Read(22938, 10, ref ia_array_WashBottle);
                if (i_state_WashBottle != -1)
                {
                    Communal._ia_d22938 = ia_array_WashBottle;
                }
            }


            i_state = FADM_Object.Communal._tcpModBus.Read(900, 50, ref ia_array);

            if (i_state != -1)
            {

                Communal._ia_d900 = ia_array;

                Communal._b_isUpdateNewData = true;

                Communal._s_plcVersion = ia_array[27].ToString("d4") + ia_array[28].ToString("d4");

                double d_b = 0.0;
                if (ia_array[1] < 0)
                {
                    d_b = ((ia_array[2] + 1) * 65536 + ia_array[1]) / 1000.0;
                }
                else
                {
                    d_b = (ia_array[2] * 65536 + ia_array[1]) / 1000.0;
                }
                //if (Lib_Card.Configure.Parameter.Machine_BalanceType == 0)
                //{
                //    d_b /= 10;
                //}


                if (Convert.ToString(d_b).Equals("9999"))
                {
                    FADM_Object.Communal._s_balanceValue = "9999";
                }
                else if (Convert.ToString(d_b).Equals("8888"))
                {
                    FADM_Object.Communal._s_balanceValue = "8888";
                }
                else if (Convert.ToString(d_b).Equals("7777"))
                {
                    FADM_Object.Communal._s_balanceValue = "7777";
                }
                else if (Convert.ToString(d_b).Equals("6666"))
                {
                    FADM_Object.Communal._s_balanceValue = "6666";
                }
                else
                {
                    FADM_Object.Communal._s_balanceValue = Convert.ToString(d_b);
                }

                //光幕信号 这上面的MyAlarm不要等待
                int i_a2 = ia_array[3];
                char[] ca_cc = Convert.ToString(i_a2, 2).PadLeft(4, '0').ToArray();
                if (ca_cc[ca_cc.Length - 1].Equals('0'))
                {
                    if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                        Lib_Card.CardObject.bStopScr = true;
                    else
                        Lib_Card.CardObject.bFront = true;
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                        Lib_Card.CardObject.bStopScr = false;
                    else
                        Lib_Card.CardObject.bFront = false;
                }

                if (ca_cc[ca_cc.Length - 3].Equals('0'))
                {
                    Lib_Card.CardObject.bRight = true;
                }
                else
                {
                    Lib_Card.CardObject.bRight = false;
                }

                if (ca_cc[ca_cc.Length - 2].Equals('0'))
                {
                    Lib_Card.CardObject.bLeft = true;
                }
                else
                {
                    Lib_Card.CardObject.bLeft = false;
                }


                if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                {
                    if (ca_cc[ca_cc.Length - 4].Equals('0'))
                    {
                        Lib_Card.CardObject.bBack = true;
                    }
                    else
                    {
                        Lib_Card.CardObject.bBack = false;
                    }
                }

            }
            else
            {
                Console.WriteLine("TCP返回值-1\n");
                if (b_istrue)
                {
                    b_istrue = false;
                    FADM_Object.Communal._tcpModBus.ReConnect();
                    goto Label1;
                }
                b_istrue = true;
                goto Label1;
            }
            return ia_array;
        }

        /// <summary>
        /// 一直读取等待标志位和光幕急停
        /// </summary>
        public static int[] AwaitSuccess( bool b_lock)
        {
            bool b_istrue = false;
            int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0,0,0,0,0,0,0,0,0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,0};
            int i_state = 0;

        Label1:
            if(b_lock)
            {
                //判断当前杯是否已经上锁止，如果没有锁止，就发没有锁止给PLC
                if (FADM_Object.Communal._i_OptCoverCupNum > 0 && FADM_Object.Communal._i_OptCoverCupNum <= Lib_Card.Configure.Parameter.Machine_Cup_Total)
                {
                    if (Dye._cup_Temps[FADM_Object.Communal._i_OptCoverCupNum - 1]._i_lockUp != 1)
                    {
                        int[] ia_array_write = { 1 };
                        int i_state_write = FADM_Object.Communal._tcpModBus.Write(859, ia_array_write);
                        if (i_state_write == -1)
                        {
                            goto Label1;
                        }
                    }
                }
            }

            i_state = FADM_Object.Communal._tcpModBus.Read(900, 50, ref ia_array);

            if (i_state != -1)
            {

                Communal._ia_d900 = ia_array;

                Communal._b_isUpdateNewData = true;

                Communal._s_plcVersion = ia_array[27].ToString("d4") + ia_array[28].ToString("d4");

                double d_b = 0.0;
                if (ia_array[1] < 0)
                {
                    d_b = ((ia_array[2] + 1) * 65536 + ia_array[1]) / 1000.0;
                }
                else
                {
                    d_b = (ia_array[2] * 65536 + ia_array[1]) / 1000.0;
                }
                //if (Lib_Card.Configure.Parameter.Machine_BalanceType == 0)
                //{
                //    d_b /= 10;
                //}


                if (Convert.ToString(d_b).Equals("9999"))
                {
                    FADM_Object.Communal._s_balanceValue = "9999";
                }
                else if (Convert.ToString(d_b).Equals("8888"))
                {
                    FADM_Object.Communal._s_balanceValue = "8888";
                }
                else if (Convert.ToString(d_b).Equals("7777"))
                {
                    FADM_Object.Communal._s_balanceValue = "7777";
                }
                else if (Convert.ToString(d_b).Equals("6666"))
                {
                    FADM_Object.Communal._s_balanceValue = "6666";
                }
                else
                {
                    FADM_Object.Communal._s_balanceValue = Convert.ToString(d_b);
                }

                //光幕信号 这上面的MyAlarm不要等待
                int i_a2 = ia_array[3];
                char[] ca_cc = Convert.ToString(i_a2, 2).PadLeft(4, '0').ToArray();
                if (ca_cc[ca_cc.Length - 1].Equals('0'))
                {
                    if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                        Lib_Card.CardObject.bStopScr = true;
                    else
                        Lib_Card.CardObject.bFront = true;
                }
                else
                {
                    if (Lib_Card.Configure.Parameter.Machine_IsStopOrFront == 0)
                        Lib_Card.CardObject.bStopScr = false;
                    else
                        Lib_Card.CardObject.bFront = false;
                }

                if (ca_cc[ca_cc.Length - 3].Equals('0'))
                {
                    Lib_Card.CardObject.bRight = true;
                }
                else
                {
                    Lib_Card.CardObject.bRight = false;
                }

                if (ca_cc[ca_cc.Length - 2].Equals('0'))
                {
                    Lib_Card.CardObject.bLeft = true;
                }
                else
                {
                    Lib_Card.CardObject.bLeft = false;
                }


                if (Lib_Card.Configure.Parameter.Machine_UseBack == 1)
                {
                    if (ca_cc[ca_cc.Length - 4].Equals('0'))
                    {
                        Lib_Card.CardObject.bBack = true;
                    }
                    else
                    {
                        Lib_Card.CardObject.bBack = false;
                    }
                }

                if (FADM_Object.Communal._b_isHaveDyeStop)
                {
                    int a23 = ia_array[23]; //
                    string s_str2 = Convert.ToString(a23, 2).PadLeft(9, '0');
                    ca_cc = s_str2.ToArray();//
                    if ((ca_cc[ca_cc.Length - 9].Equals('0') && !FADM_Object.Communal._b_isDyeStop) || (ca_cc[ca_cc.Length - 9].Equals('1') && FADM_Object.Communal._b_isDyeStop))
                    {
                        if (FADM_Object.Communal._tcpDyeHMI1 != null)
                            FADM_Object.Communal._tcpDyeHMI1.b_isSendStop = false;
                        if (FADM_Object.Communal._tcpDyeHMI2 != null)
                            FADM_Object.Communal._tcpDyeHMI2.b_isSendStop = false;
                        if (FADM_Object.Communal._tcpDyeHMI3 != null)
                            FADM_Object.Communal._tcpDyeHMI3.b_isSendStop = false;
                        if (FADM_Object.Communal._tcpDyeHMI4 != null)
                            FADM_Object.Communal._tcpDyeHMI4.b_isSendStop = false;
                        if (FADM_Object.Communal._tcpDyeHMI5 != null)
                            FADM_Object.Communal._tcpDyeHMI5.b_isSendStop = false;
                        if (FADM_Object.Communal._tcpDyeHMI6 != null)
                            FADM_Object.Communal._tcpDyeHMI6.b_isSendStop = false;
                        FADM_Object.Communal._b_isDyeStop = ca_cc[ca_cc.Length - 9].Equals('0') ? true : false;
                    }
                }

            }
            else
            {
                Console.WriteLine("TCP返回值-1\n");
                if (b_istrue)
                {
                    b_istrue = false;
                    FADM_Object.Communal._tcpModBus.ReConnect();
                    goto Label1;
                }
                b_istrue = true;
                goto Label1;
            }
            return ia_array;
        }


        /// <summary>
        /// 设置批次启动
        /// </summary>
        public static void SetBatchStart()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false); //恢复
                                                            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 1 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(808, ia_array3);
                if (i_state != -1)
                {
                    //清除标志位成功 重新读一下
                    i_state = FADM_Object.Communal._tcpModBus.Read(808, 1, ref ia_array3);
                    if (i_state != -1 && ia_array3[0] == 1)
                    {
                        FADM_Object.Communal.WriteTcpStatus(true); //恢复
                        return;
                    }
                    else
                    {
                        goto label2;
                    }
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }
        /// <summary>
        /// 设置批次关闭
        /// </summary>
        public static void SetBatchClose()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false); //恢复
                                                            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 2 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(808, ia_array3);
                if (i_state != -1)
                {
                    //清除标志位成功 重新读一下
                    i_state = FADM_Object.Communal._tcpModBus.Read(808, 1, ref ia_array3);
                    if (i_state != -1 && ia_array3[0] == 2)
                    {
                        FADM_Object.Communal.WriteTcpStatus(true); //恢复
                        return;
                    }
                    else
                    {
                        goto label2;
                    }
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }


        /// <summary>
        /// 复位
        /// </summary>
        public static int MyMachineReset()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                new Reset().MachineReset();
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行复位");
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位

                lableTop:

                    int i_xPules = 0;
                    int i_yPules = 0;

                    if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1 || (!Communal._b_isBalanceInDrip))
                    {
                        if (FADM_Object.Communal._b_isNewSet)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (FADM_Object.Communal._i_optBottleNum - 1) %
                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (FADM_Object.Communal._i_optBottleNum - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                        else
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (FADM_Object.Communal._i_optBottleNum - 1) %
                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (FADM_Object.Communal._i_optBottleNum - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                    }
                    else
                    {
                        int iNo = FADM_Object.Communal._i_optBottleNum;
                        if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14 >= iNo)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 7 >= iNo)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                        }
                        else
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                               ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                               * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                    }
                    if (FADM_Object.Communal._i_optBottleNum == 999)
                    {
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_Y;
                    }
                    if (FADM_Object.Communal._i_optBottleNum == 0)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                        FADM_Object.Communal.WriteMachineStatus(0);
                        FADM_Object.Communal.WriteTcpStatus(true); //天平先不要轮询
                        return 0;
                    }
                    int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                    i_d0 = i_xPules;
                    i_d2 = i_d0 / 65536;
                    if (i_d0 < 0) //负数脉冲
                    {
                        if (i_d2 == 0)
                        {
                            i_d2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_d0) > 65536)
                            {
                                i_d2 = i_d2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_d2 = i_d0 / 65536;
                    }
                    i_d1 = i_d0 % 65536;

                    int i_yd0 = 0, i_yd1 = 0, i_yd2 = 0;
                    i_yd0 = i_yPules;
                    i_yd2 = i_yd0 / 65536;
                    if (i_yd0 < 0) //负数脉冲
                    {
                        if (i_yd2 == 0)
                        {
                            i_yd2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_yd0) > 65536)
                            {
                                i_yd2 = i_yd2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_yd2 = i_yd0 / 65536;
                    }
                    i_yd1 = i_yd0 % 65536;
                    int[] ia_array = { 9, i_d1, i_d2, i_yd1, i_yd2, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {

                        //判断错误返回值
                        if (GetReturn(0) == -2)
                        {
                            return -2;
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                            FADM_Object.Communal.WriteMachineStatus(0);
                        }

                    }
                    else
                    {
                        Console.WriteLine("复位编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }


        }

        /// <summary>
        /// 复位
        /// </summary>
        public static int MyMachineReset1()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                new Reset().MachineReset1();
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行复位");
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位

                lableTop:

                    int i_xPules = 0;
                    int i_yPules = 0;

                    if (Lib_Card.Configure.Parameter.Other_IsOnlyDrip == 1 || (!Communal._b_isBalanceInDrip))
                    {
                        if (FADM_Object.Communal._b_isNewSet)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (FADM_Object.Communal._i_optBottleNum - 1) %
                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (FADM_Object.Communal._i_optBottleNum - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                        else
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (FADM_Object.Communal._i_optBottleNum - 1) %
                            Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y - (FADM_Object.Communal._i_optBottleNum - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                    }
                    else
                    {
                        int iNo = FADM_Object.Communal._i_optBottleNum;
                        if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14 >= iNo)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X - (iNo - 1) %
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y + (iNo - 1) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                        else if (Lib_Card.Configure.Parameter.Machine_Bottle_Total - 7 >= iNo)
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                                ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 2)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;


                        }
                        else
                        {
                            i_xPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_X -
                               ((iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) % 8 + 3)
                               * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                            i_yPules = Lib_Card.Configure.Parameter.Coordinate_Bottle_Y +
                                ((Lib_Card.Configure.Parameter.Machine_Bottle_Total - 14) /
                                Lib_Card.Configure.Parameter.Machine_Bottle_Column +
                                (iNo + 14 - Lib_Card.Configure.Parameter.Machine_Bottle_Total) / 8)
                                * Lib_Card.Configure.Parameter.Coordinate_Bottle_Interval;
                        }
                    }
                    if (FADM_Object.Communal._i_optBottleNum == 999)
                    {
                        i_xPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_X;
                        i_yPules = Lib_Card.Configure.Parameter.Coordinate_Syringes_Y;
                    }
                    if (FADM_Object.Communal._i_optBottleNum == 0)
                    {
                        FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                        //FADM_Object.Communal.WriteMachineStatus(0);
                        FADM_Object.Communal.WriteTcpStatus(true); //天平先不要轮询
                        return 0;
                    }
                    int i_d0 = 0, i_d1 = 0, i_d2 = 0;
                    i_d0 = i_xPules;
                    i_d2 = i_d0 / 65536;
                    if (i_d0 < 0) //负数脉冲
                    {
                        if (i_d2 == 0)
                        {
                            i_d2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_d0) > 65536)
                            {
                                i_d2 = i_d2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_d2 = i_d0 / 65536;
                    }
                    i_d1 = i_d0 % 65536;

                    int i_yd0 = 0, i_yd1 = 0, i_yd2 = 0;
                    i_yd0 = i_yPules;
                    i_yd2 = i_yd0 / 65536;
                    if (i_yd0 < 0) //负数脉冲
                    {
                        if (i_yd2 == 0)
                        {
                            i_yd2 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_yd0) > 65536)
                            {
                                i_yd2 = i_yd2 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        i_yd2 = i_yd0 / 65536;
                    }
                    i_yd1 = i_yd0 % 65536;
                    int[] ia_array = { 9, i_d1, i_d2, i_yd1, i_yd2, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {

                        //判断错误返回值
                        if (GetReturn(0) == -2)
                        {
                            return -2;
                        }
                        else
                        {
                            FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                            //FADM_Object.Communal.WriteMachineStatus(0);
                        }

                    }
                    else
                    {
                        Console.WriteLine("复位编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }



        /// <summary>
        /// 相对移动
        /// </summary>
        /// <param name="i_axis">轴 1.X轴 2.Y轴 3.Z轴</param>
        /// <param name="iPosition">脉冲</param>
        /// <returns></returns>
        public static int TargetMoveRelative(int i_axis, int i_pules, int i_hSpeed, int i_uTime, int i_lSpeed)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                if (i_axis== 1)
                {
                    Lib_Card.Base.Card.MoveArg s_MoveArg = new Lib_Card.Base.Card.MoveArg()
                    {
                        Pulse = i_pules,
                        LSpeed = i_lSpeed,
                        HSpeed = i_hSpeed,
                        Time = i_uTime
                    };
                    if (-1 == Lib_Card.CardObject.OA1Axis.Relative_X(Lib_Card.Configure.Parameter.Machine_CylinderVersion, s_MoveArg, 0))
                        throw new Exception("驱动异常");
                }
                else if (i_axis == 2)
                {
                    Lib_Card.Base.Card.MoveArg s_MoveArg = new Lib_Card.Base.Card.MoveArg()
                    {
                        Pulse = i_pules,
                        LSpeed = i_lSpeed,
                        HSpeed = i_hSpeed,
                        Time = i_uTime
                    };
                    if (-1 == Lib_Card.CardObject.OA1Axis.Relative_Y(Lib_Card.Configure.Parameter.Machine_CylinderVersion, s_MoveArg, 0))
                        throw new Exception("驱动异常");
                }
                else if (i_axis == 3)
                {
                    Lib_Card.Base.Card.MoveArg s_MoveArg = new Lib_Card.Base.Card.MoveArg()
                    {
                        Pulse = i_pules,
                        LSpeed = i_lSpeed,
                        HSpeed = i_hSpeed,
                        Time = i_uTime
                    };
                    if (-1 == Lib_Card.CardObject.OA1Axis.Relative_Z( s_MoveArg, 0))
                        throw new Exception("驱动异常");
                }
                return 0;
            }
            else
            {
                try
                {
                    Console.WriteLine("开始相对移动");
                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    if (!FADM_Object.Communal._b_auto)
                    {
                        for (int i = 0; i < Communal._ia_d900.Length; i++) { Communal._ia_d900[i] = 0; }
                        Communal._b_isUpdateNewData = false;
                        //等待调试页面停止刷新
                        Thread.Sleep(200);
                    }
                    ClearSuccessState();//先清除标志位
                    Console.WriteLine("清除完标志位");
                lableTop:
                    /*if (!FADM_Object.Communal._b_auto) {//在手动页面 等待手动页面退出
                        goto Label1;
                    }*/
                    int[] ia_array;

                    int d_1 = 0;
                    d_1 = i_pules / 65536;
                    if (i_pules < 0) //负数脉冲
                    {
                        if (d_1 == 0)
                        {
                            d_1 = -1;
                        }
                        else
                        {
                            if (Math.Abs(i_pules) > 65536)
                            {
                                d_1 = d_1 + -1;
                            }
                        }
                    }
                    else
                    {  //正数脉冲
                        d_1 = i_pules / 65536;
                    }
                    i_pules = i_pules % 65536;


                    int d_2 = 0;
                    if (i_hSpeed > 65536)
                    {
                        d_2 = i_hSpeed / 65536;
                        i_hSpeed = i_hSpeed % 65536;
                    }
                    int d_3 = 0;
                    if (i_uTime > 65536)
                    {
                        d_3 = i_uTime / 65536;
                        i_uTime = i_uTime % 65536;
                    }


                    if (i_axis == 1)
                    {
                        ia_array = new int[] { 8, i_pules, d_1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, i_hSpeed, d_2, i_uTime, d_3 };
                    }
                    else if (i_axis == 2)
                    {
                        ia_array = new int[] { 8, 0, 0, i_pules, d_1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, i_hSpeed, d_2, i_uTime, d_3 };
                    }
                    else
                    {
                        ia_array = new int[] { 8, 0, 0, 0, 0, i_pules, d_1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, i_hSpeed, d_2, i_uTime, d_3 };
                    }
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Console.WriteLine("写入相对移动编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2"))
                    {
                        throw e;
                    }
                    else
                    {
                        MessageBox.Show(e.ToString());
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }




        /// <summary>
        /// 检查针筒
        /// </summary>
        /// <returns></returns>
        public static int Reset()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                OpenOrCloseBear();
                FADM_Auto.Reset.IOReset();
                return 0; 
            }
            else
            {
                try
                {
                    Lib_Log.Log.writeLogException("检查针筒方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询 
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int[] ia_array = { 10, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入检查复位动作编号返回失败,继续写入");
                        Console.WriteLine("写入检查复位动作编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 检查针筒
        /// </summary>
        /// <returns></returns>
        public static int ResetClosing()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                FADM_Auto.Reset.IOReset();
                return 0;
            }
            else
            {
                try
                {
                    Lib_Log.Log.writeLogException("检查针筒方法");

                    bool istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询 
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int[] ia_array = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    int i_state = 0;
                    i_state = FADM_Object.Communal._tcpModBus.Read(900, 26, ref ia_array);
                    if (i_state != -1)
                    {
                        int a5 = ia_array[5]; //输入点(双字)
                        string str2 = Convert.ToString(a5, 2).PadLeft(15, '0');
                        if (!str2.Equals("0"))
                        {
                            char[] cc = str2.ToArray();//急停
                            if (cc[cc.Length - 6].Equals('1') ? true : false) //针筒传感器
                            {
                                throw new Exception("发现针筒或杯盖");
                            }
                        }
                    }
                    else
                    {
                        goto lableTop;
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("发现针筒或杯盖"))
                    {
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }



        /// <summary>
        /// 清除动作错误编号
        /// </summary>
        /// <returns></returns>
        public static void ClearError()
        {
            Lib_Log.Log.writeLogException("清除动作错误编号");
            bool b_istrue = false;
            FADM_Object.Communal.WriteTcpStatus(false); //恢复
        //清掉执行完成 标志位
        label2:
            int[] ia_array3 = { 0 };
            int i_state = FADM_Object.Communal._tcpModBus.Write(900, ia_array3);
            if (i_state != -1)
            {
                //清除标志位成功 重新读一下
                i_state = FADM_Object.Communal._tcpModBus.Read(900, 1, ref ia_array3);
                if (i_state != -1 && ia_array3[0] == 0)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    return;
                }
                else
                {
                    goto label2;
                }
            }
            else
            {
                if (b_istrue)
                {
                    b_istrue = false;
                    FADM_Object.Communal._tcpModBus.ReConnect();
                    goto label2;
                }
                b_istrue = true;
                goto label2;
            }

        }


        /// <summary>
        /// 天平清零 发送指令成功 就退出此方法 。后面有方法判断是否是0
        /// </summary>
        /// <returns></returns>
        public static void ClearBalance()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false);
            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 1 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(827, ia_array3);
                if (i_state != -1)
                {
                    FADM_Object.Communal.WriteTcpStatus(true);//恢复
                    /*FADM_Object.Communal.WriteTcpStatus(true); //恢复
                  //读天平数据看下是否等于0
                lable3:
                    if (Convert.ToDouble(FADM_Object.Communal._s_balanceValue) == 0.00)
                    {
                        return;
                    }
                    else {
                        goto lable3;
                    }*/
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }



        /// <summary>
        /// 天平复位
        /// </summary>
        /// <returns></returns>
        public static void ResetBalance()
        {
            return;
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                return;
            }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false);
            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 30 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(811, ia_array3);
                if (i_state != -1)
                {
                    FADM_Object.Communal.WriteTcpStatus(true);//恢复
                    /*FADM_Object.Communal.WriteTcpStatus(true); //恢复
                  //读天平数据看下是否等于0
                lable3:
                    if (Convert.ToDouble(FADM_Object.Communal._s_balanceValue) == 0.00)
                    {
                        return;
                    }
                    else {
                        goto lable3;
                    }*/
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }

        /// <summary>
        /// 移动针筒
        /// </summary>
        /// <returns></returns>
        public static int MoveSyringes(int i_xStartPules, int i_yStartPules)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                // 移动到针筒处
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xStartPules, i_yStartPules, 1);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                //拿针筒
                Lib_Card.ADT8940A1.Module.GetOrPutClamp getOrPutClamp = new Lib_Card.ADT8940A1.Module.GetOrPutClamp();
                getOrPutClamp.GetClamp(Lib_Card.Configure.Parameter.Machine_CylinderVersion);

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    //if (!FADM_Object.Communal._b_auto)
                    //{//在手动页面 等待手动页面退出
                    //    goto Label1;
                    //}
                    Lib_Log.Log.writeLogException("执行拿针筒方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }

                    int[] ia_array = { 14, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1, 0, 1 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入拿针筒动作编号返回失败,继续写入");
                        Console.WriteLine("拿针筒编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现针筒");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 拿针筒，用于清洗针筒
        /// </summary>
        /// <returns></returns>
        public static int GetSyringesWash(int i_xStartPules, int i_yStartPules)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                // 移动到针筒处
                Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.TargeMove();
                int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0, i_xStartPules, i_yStartPules, 1);
                if (-1 == iMove)
                    throw new Exception("驱动异常");
                else if (-2 == iMove)
                    throw new Exception("收到退出消息");
                //拿针筒
                Lib_Card.ADT8940A1.Module.GetOrPutClamp getOrPutClamp = new Lib_Card.ADT8940A1.Module.GetOrPutClamp();
                getOrPutClamp.GetClamp(Lib_Card.Configure.Parameter.Machine_CylinderVersion);

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    //if (!FADM_Object.Communal._b_auto)
                    //{//在手动页面 等待手动页面退出
                    //    goto Label1;
                    //}
                    Lib_Log.Log.writeLogException("执行拿针筒方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }

                    int[] ia_array = { 14, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 0, 1 ,0,1};
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入拿针筒动作编号返回失败,继续写入");
                        Console.WriteLine("拿针筒编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现针筒");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 清洗针筒动作
        /// </summary>
        /// <returns></returns>
        public static int WashSyringes(int i_syringeType)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //气缸到阻挡位
                WashSyringe cylinderMo = new WashSyringe();
                if (-1 == cylinderMo.Wash(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType))
                    return -1;

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    //if (!FADM_Object.Communal._b_auto)
                    //{//在手动页面 等待手动页面退出
                    //    goto Label1;
                    //}
                    Lib_Log.Log.writeLogException("执行清洗针筒方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int[] ia_array = { 20,0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0,  0, i_syringeType };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入清洗针筒动作编号返回失败,继续写入");
                        Console.WriteLine("清洗针筒编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现抓手");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }
        }

        /// <summary>
        /// 流量计清零
        /// </summary>
        /// <returns></returns>
        public static void ClearFlow()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false);
            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 1 };
                int i_state = FADM_Object.Communal._tcpModBus.Write(857, ia_array3);
                if (i_state != -1)
                {
                    FADM_Object.Communal.WriteTcpStatus(true);//恢复
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }

        /// <summary>
        /// 读流量计脉冲
        /// </summary>
        /// <returns></returns>
        public static void ReadFlow()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            { }
            else
            {
                bool b_istrue = false;
                FADM_Object.Communal.WriteTcpStatus(false);
            //清掉执行完成 标志位
            label2:
                int[] ia_array3 = { 0, 0 };
                int i_state = FADM_Object.Communal._tcpModBus.Read(933, 2, ref ia_array3);
                if (i_state != -1)
                {
                    //记录脉冲数据
                    int d_b = 0;
                    if (ia_array3[0] < 0)
                    {
                        d_b = ((ia_array3[1] + 1) * 65536 + ia_array3[0]);
                    }
                    else
                    {
                        d_b = (ia_array3[1] * 65536 + ia_array3[0]);
                    }
                    FADM_Object.Communal.i_flowPulse = d_b;
                    FADM_Object.Communal.WriteTcpStatus(true);//恢复
                }
                else
                {
                    if (b_istrue)
                    {
                        b_istrue = false;
                        FADM_Object.Communal._tcpModBus.ReConnect();
                        goto label2;
                    }
                    b_istrue = true;
                    goto label2;
                }
            }
        }

        /// <summary>
        /// 移动母液瓶
        /// </summary>
        /// <param name="i_xStartPules">开始X坐标</param>
        /// <param name="i_yStartPules">开始Y坐标</param>
        /// <param name="i_xEndPules">结束X坐标</param>
        /// <param name="i_yEndPules">结束Y坐标</param>
        /// <returns></returns>
        public static int MoveBottle(int i_xStartPules, int i_yStartPules, int i_xEndPules, int i_yEndPules,int i_type)
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {
                        //在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("执行取放瓶方法");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }
                    int d_3 = 0;
                    if (i_xEndPules > 65536)
                    {
                        d_3 = i_xEndPules / 65536;
                        i_xEndPules = i_xEndPules % 65536;
                    }
                    int d_4 = 0;
                    if (i_yEndPules > 65536)
                    {
                        d_4 = i_yEndPules / 65536;
                        i_yEndPules = i_yEndPules % 65536;
                    }
                    int[] ia_array = { 26, i_xStartPules, d_1, i_yStartPules, d_2, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, i_xEndPules, d_3, i_yEndPules, d_4,0,0,0,0,0,i_type};
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入取放瓶动作编号返回失败,继续写入");
                        Console.WriteLine("取放瓶编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 5411)
                            {
                                throw new Exception("放瓶时气缸下失败");
                            }
                            else if (ia_errArray[i] == 5412)
                            {
                                throw new Exception("放瓶失败");
                            }
                            else if (ia_errArray[i] == 5413)
                            {
                                throw new Exception("在洗瓶工位取瓶失败");
                            }
                            else if (ia_errArray[i] == 5414)
                            {
                                throw new Exception("在转盘工位取瓶失败");
                            }
                            else if (ia_errArray[i] == 5415)
                            {
                                throw new Exception("放瓶到洗瓶工位失败");
                            }
                            else if (ia_errArray[i] == 5416)
                            {
                                throw new Exception("放瓶到转盘工位失败");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }

        /// <summary>
        /// 加粉
        /// </summary>
        /// <returns></returns>
        public static int AddPower()
        {

            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("加粉");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    

                    int[] ia_array = { 27 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入加粉动作编号返回失败,继续写入");
                        Console.WriteLine("加粉编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }


                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 5421)
                            {
                                throw new Exception("加粉超时");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }

        /// <summary>
        /// PH抽液
        /// </summary>
        /// <returns></returns>
        public static int PHExtract(int i_xStartPules, int i_yStartPules)
        {

            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
               
                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("PH抽液");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }

                    int d3 = 0;
                    int i_pluse = Lib_Card.Configure.Parameter.Other_PHExtractPluse;
                    d3 = i_pluse / 65536;
                    i_pluse = i_pluse % 65536;

                    int[] ia_array = { 24, i_xStartPules, d_1, i_yStartPules, d_2, i_pluse, d3};
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入PH抽液动作编号返回失败,继续写入");
                        Console.WriteLine("PH抽液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }

                   
                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现抓手");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }

        /// <summary>
        /// PH排液
        /// </summary>
        /// <returns></returns>
        public static int PHShove(int i_xStartPules, int i_yStartPules)
        {

            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {

                return 0;
            }
            else
            {
                try
                {
                Label1:
                    if (!FADM_Object.Communal._b_auto)
                    {//在手动页面 等待手动页面退出
                        goto Label1;
                    }
                    Lib_Log.Log.writeLogException("PH排液");

                    bool b_istrue = false;
                    FADM_Object.Communal.WriteTcpStatus(false); //天平先不要轮询
                    ClearSuccessState();//先清除标志位
                    Lib_Log.Log.writeLogException("清除标志位结束");

                lableTop:
                    int d_1 = 0;
                    if (i_xStartPules > 65536)
                    {
                        d_1 = i_xStartPules / 65536;
                        i_xStartPules = i_xStartPules % 65536;
                    }
                    int d_2 = 0;
                    if (i_yStartPules > 65536)
                    {
                        d_2 = i_yStartPules / 65536;
                        i_yStartPules = i_yStartPules % 65536;
                    }


                    int[] ia_array = { 25, i_xStartPules, d_1, i_yStartPules, d_2 };
                    int i_state = FADM_Object.Communal._tcpModBus.Write(800, ia_array);
                    if (i_state != -1)
                    {
                        //判断错误返回值
                        if (GetReturn(1) == -2)
                        {
                            return -2;
                        }
                        else
                        {

                        }
                    }
                    else
                    {
                        Lib_Log.Log.writeLogException("写入排液动作编号返回失败,继续写入");
                        Console.WriteLine("排液编号返回失败,继续写入");
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }


                    return 0;
                }
                catch (Exception e)
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                    if (e.Message.Equals("-2") || e.Message.Equals("10") || e.Message.Equals("未发现杯盖") || e.Message.Equals("发现杯盖或针筒") || e.Message.Equals("抓手A夹紧异常") || e.Message.Equals("抓手B夹紧异常"))
                    {
                        int[] ia_errArray = new int[100];
                    Label123:
                        int i_state1 = MyModbusFun.GetErrMsgNew(ref ia_errArray);
                        if (i_state1 == -1)
                            goto Label123;
                        for (int i = 0; i < ia_errArray.Length; i++)
                        {
                            if (ia_errArray[i] == 4501)
                            {
                                throw new Exception("未发现抓手");
                            }
                            else if (ia_errArray[i] == 4502)
                            {
                                throw new Exception("取布夹具时抓手关失败");
                            }
                            else if (ia_errArray[i] == 4503)
                            {
                                throw new Exception("取布夹具时气缸下失败");
                            }
                            else if (ia_errArray[i] == 2701)
                            {
                                throw new Exception("发现杯盖或针筒");
                            }

                        }
                        throw e;
                    }
                    else
                    {
                        throw new Exception("-1");
                    }
                }
                finally
                {
                    FADM_Object.Communal.WriteTcpStatus(true); //恢复
                }
            }

        }

        /// <summary>
        /// 解锁6工位
        /// </summary>
        /// <returns></returns>
        public static int UnLockSix()
        {
            if (Lib_Card.Configure.Parameter.Machine_Type == 0)
            {
                //OpenOrCloseBear();
                //Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                //int iMRes = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, i_syringeType);
                //if (-1 == iMRes)
                //    throw new Exception("驱动异常");
                //else if (-2 == iMRes)
                //    throw new Exception("收到退出消息");
                return 0;
            }
            else
            {
                try
                {
                Label1:

                    bool b_istrue = false;

                lableTop:
                    int[] ia_array = { 1};
                    int i_state = FADM_Object.Communal._tcpModBus.Write(862, ia_array);
                    if (i_state != -1)
                    {
                        
                    }
                    else
                    {
                        
                        if (b_istrue)
                        {
                            b_istrue = false;
                            FADM_Object.Communal._tcpModBus.ReConnect();
                            goto lableTop;
                        }
                        b_istrue = true;
                        goto lableTop;
                        //throw new Exception("10");
                    }
                    return 0;
                }
                catch (Exception e)
                {
                    return 0;
                }
                finally
                {
                    
                }
            }
        }
    }
}
