using Lib_Card.ADT8940A1;
using Lib_File;
using SmartDyeing.FADM_Object;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SmartDyeing.FADM_Auto
{
    internal class Reset
    {
        public void MachineReset()
        {
            try
            {
                FADM_Object.Communal.WriteMachineStatus(10);
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位启动");
                Lib_Card.ADT8940A1.Axis.Axis.Axis_Exit = false;
                Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                //打开搅拌
                Lib_Card.ADT8940A1.OutPut.Blender.Blender blender = new Lib_Card.ADT8940A1.OutPut.Blender.Blender_Basic();
                if (-1 == blender.Blender_Off())
                    throw new Exception("驱动异常");

                //关闭废液回抽
                Lib_Card.ADT8940A1.OutPut.Waste.Waste waste = new Lib_Card.ADT8940A1.OutPut.Waste.Waste_Basic();
                if (-1 == waste.Waste_Off())
                    throw new Exception("驱动异常");

                //判断是否气缸下运行

                bool blExtractionOrPut = false;
                if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                {
                    int iCylinderDown = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Down);
                    if (-1 == iCylinderDown)
                        throw new Exception("驱动异常");
                    else if (1 == iCylinderDown)
                        blExtractionOrPut = true;
                }
                else
                {

                    int iCylinderUp = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Up);
                    if (-1 == iCylinderUp)
                        throw new Exception("驱动异常");

                    int iCylinderDown = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Down);
                    if (-1 == iCylinderDown)
                        throw new Exception("驱动异常");

                    if (0 == iCylinderUp && 1 == iCylinderDown)
                        blExtractionOrPut = true;
                }



                if (blExtractionOrPut)
                {

                    //在抽液或放针环节
                    int iSyringe = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                    if (Lib_Card.Configure.Parameter.Machine_isSyringe==1)
                    {
                        iSyringe = 1;
                    }
                    if (-1 == iSyringe)
                        throw new Exception("驱动异常");
                    else if (1 == iSyringe)
                    {
                        //放针
                        string sSql = "SELECT * FROM bottle_details WHERE BottleNum = " + FADM_Object.Communal._i_optBottleNum + ";";
                        DataTable dataTable1 = FADM_Object.Communal._fadmSqlserver.GetData(sSql);
                        string sSyringeType = Convert.ToString(dataTable1.Rows[0]["SyringeType"]);

                        Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                        int iPut = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, sSyringeType == "小针筒" ? 0 : 1);
                        if (-1 == iPut)
                            throw new Exception("驱动异常");
                        else if (-2 == iPut)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        //抓手打开
                        Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Condition();
                        if (-1 == tongs.Tongs_Off())
                            throw new Exception("驱动异常");

                        //气缸上
                        Lib_Card.ADT8940A1.OutPut.Cylinder.Cylinder cylinder;
                        if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.SingleControl.Cylinder_Condition();
                        else
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.DualControl.Cylinder_Condition();

                        if (-1 == cylinder.CylinderUp(0))
                            throw new Exception("驱动异常");
                    }

                }
                else
                {
                    //不在抽液或放针环节
                    int iSyringe = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                    if (Lib_Card.Configure.Parameter.Machine_isSyringe==1)
                    {
                        iSyringe = 1;
                    }

                    if (-1 == iSyringe)
                        throw new Exception("驱动异常");
                    else if (1 == iSyringe)
                    {
                        //有针筒

                        //抓手关闭
                        Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Condition();
                        if (-1 == tongs.Tongs_On())
                            throw new Exception("驱动异常");

                        //停止Z轴
                        if (-1 == Lib_Card.CardObject.OA1.SuddnStop(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z))
                            throw new Exception("驱动异常");

                        //气缸上
                        Lib_Card.ADT8940A1.OutPut.Cylinder.Cylinder cylinder;
                        if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.SingleControl.Cylinder_Condition();
                        else
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.DualControl.Cylinder_Condition();

                        if (-1 == cylinder.CylinderUp(0))
                            throw new Exception("驱动异常");

                        //接液盘伸出
                        Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Condition();
                        if (-1 == tray.Tray_On())
                            throw new Exception("驱动异常");

                        //回到母液瓶
                        int i_mRes1 = MyModbusFun.TargetMove(0, FADM_Object.Communal._i_optBottleNum, 1);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");

                        //放针
                        string sSql = "SELECT * FROM bottle_details WHERE BottleNum = " + FADM_Object.Communal._i_optBottleNum + ";";
                        DataTable dataTable1 = FADM_Object.Communal._fadmSqlserver.GetData(sSql);
                        string sSyringeType = Convert.ToString(dataTable1.Rows[0]["SyringeType"]);

                        Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                        int iPut = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, sSyringeType == "小针筒" ? 0 : 1);
                        if (-1 == iPut)
                            throw new Exception("驱动异常");
                        else if (-2 == iPut)
                            throw new Exception("收到退出消息");
                    }

                }

                //回到停止位
                Lib_Card.ADT8940A1.Module.Move.Move move1 = new Lib_Card.ADT8940A1.Module.Move.Move_Standby();
                FADM_Object.Communal._i_OptCupNum = 0;
                FADM_Object.Communal._i_optBottleNum = 0;
                //int iMove1 = move1.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, 0);
                //if (-1 == iMove1)
                //    throw new Exception("驱动异常");

                int i_mRes = MyModbusFun.TargetMove(3, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");


                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                FADM_Object.Communal.WriteMachineStatus(8);
                FADM_Form.CustomMessageBox.Show(ex.Message, "MachineReset", MessageBoxButtons.OK, true);

            }
        }

        public void MachineReset1()
        {
            try
            {
                //FADM_Object.Communal.WriteMachineStatus(10);
                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位启动");
                Lib_Card.ADT8940A1.Axis.Axis.Axis_Exit = false;
                Lib_SerialPort.Balance.METTLER.bReSetSign = true;
                //打开搅拌
                Lib_Card.ADT8940A1.OutPut.Blender.Blender blender = new Lib_Card.ADT8940A1.OutPut.Blender.Blender_Basic();
                if (-1 == blender.Blender_Off())
                    throw new Exception("驱动异常");

                //关闭废液回抽
                Lib_Card.ADT8940A1.OutPut.Waste.Waste waste = new Lib_Card.ADT8940A1.OutPut.Waste.Waste_Basic();
                if (-1 == waste.Waste_Off())
                    throw new Exception("驱动异常");

                //判断是否气缸下运行

                bool blExtractionOrPut = false;
                if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                {
                    int iCylinderDown = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Down);
                    if (-1 == iCylinderDown)
                        throw new Exception("驱动异常");
                    else if (1 == iCylinderDown)
                        blExtractionOrPut = true;
                }
                else
                {

                    int iCylinderUp = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Up);
                    if (-1 == iCylinderUp)
                        throw new Exception("驱动异常");

                    int iCylinderDown = Lib_Card.CardObject.OA1.ReadOutPut(Lib_Card.ADT8940A1.ADT8940A1_IO.OutPut_Cylinder_Down);
                    if (-1 == iCylinderDown)
                        throw new Exception("驱动异常");

                    if (0 == iCylinderUp && 1 == iCylinderDown)
                        blExtractionOrPut = true;
                }



                if (blExtractionOrPut)
                {

                    //在抽液或放针环节
                    int iSyringe = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                    if (Lib_Card.Configure.Parameter.Machine_isSyringe == 1)
                    {
                        iSyringe = 1;
                    }
                    if (-1 == iSyringe)
                        throw new Exception("驱动异常");
                    else if (1 == iSyringe)
                    {
                        //放针
                        string sSql = "SELECT * FROM bottle_details WHERE BottleNum = " + FADM_Object.Communal._i_optBottleNum + ";";
                        DataTable dataTable1 = FADM_Object.Communal._fadmSqlserver.GetData(sSql);
                        string sSyringeType = Convert.ToString(dataTable1.Rows[0]["SyringeType"]);

                        Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                        int iPut = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, sSyringeType == "小针筒" ? 0 : 1);
                        if (-1 == iPut)
                            throw new Exception("驱动异常");
                        else if (-2 == iPut)
                            throw new Exception("收到退出消息");
                    }
                    else
                    {
                        //抓手打开
                        Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Condition();
                        if (-1 == tongs.Tongs_Off())
                            throw new Exception("驱动异常");

                        //气缸上
                        Lib_Card.ADT8940A1.OutPut.Cylinder.Cylinder cylinder;
                        if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.SingleControl.Cylinder_Condition();
                        else
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.DualControl.Cylinder_Condition();

                        if (-1 == cylinder.CylinderUp(0))
                            throw new Exception("驱动异常");
                    }

                }
                else
                {
                    //不在抽液或放针环节
                    int iSyringe = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
                    if (Lib_Card.Configure.Parameter.Machine_isSyringe == 1)
                    {
                        iSyringe = 1;
                    }

                    if (-1 == iSyringe)
                        throw new Exception("驱动异常");
                    else if (1 == iSyringe)
                    {
                        //有针筒

                        //抓手关闭
                        Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Condition();
                        if (-1 == tongs.Tongs_On())
                            throw new Exception("驱动异常");

                        //停止Z轴
                        if (-1 == Lib_Card.CardObject.OA1.SuddnStop(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z))
                            throw new Exception("驱动异常");

                        //气缸上
                        Lib_Card.ADT8940A1.OutPut.Cylinder.Cylinder cylinder;
                        if (0 == Lib_Card.Configure.Parameter.Machine_CylinderVersion)
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.SingleControl.Cylinder_Condition();
                        else
                            cylinder = new Lib_Card.ADT8940A1.OutPut.Cylinder.DualControl.Cylinder_Condition();

                        if (-1 == cylinder.CylinderUp(0))
                            throw new Exception("驱动异常");

                        //接液盘伸出
                        Lib_Card.ADT8940A1.OutPut.Tray.Tray tray = new Lib_Card.ADT8940A1.OutPut.Tray.Tray_Condition();
                        if (-1 == tray.Tray_On())
                            throw new Exception("驱动异常");

                        //回到母液瓶
                        //Lib_Card.ADT8940A1.Module.Move.Move move = new Lib_Card.ADT8940A1.Module.Move.Move_Bottle();
                        //int iMove = move.TargetMove(Lib_Card.Configure.Parameter.Machine_CylinderVersion, FADM_Object.Communal._i_optBottleNum);
                        //if (-1 == iMove)
                        //    throw new Exception("驱动异常");
                        //else if (-2 == iMove)
                        //    throw new Exception("收到退出消息");

                        int i_mRes1 = MyModbusFun.TargetMove(0, FADM_Object.Communal._i_optBottleNum, 0);
                        if (-2 == i_mRes1)
                            throw new Exception("收到退出消息");

                        //放针
                        string sSql = "SELECT * FROM bottle_details WHERE BottleNum = " + FADM_Object.Communal._i_optBottleNum + ";";
                        DataTable dataTable1 = FADM_Object.Communal._fadmSqlserver.GetData(sSql);
                        string sSyringeType = Convert.ToString(dataTable1.Rows[0]["SyringeType"]);

                        Lib_Card.ADT8940A1.Module.Put.Put put = new Lib_Card.ADT8940A1.Module.Put.Put_Condition();
                        int iPut = put.PutSyringe(Lib_Card.Configure.Parameter.Machine_CylinderVersion, sSyringeType == "小针筒" ? 0 : 1);
                        if (-1 == iPut)
                            throw new Exception("驱动异常");
                        else if (-2 == iPut)
                            throw new Exception("收到退出消息");
                    }

                }

                //回到停止位
                Lib_Card.ADT8940A1.Module.Move.Move move1 = new Lib_Card.ADT8940A1.Module.Move.Move_Standby();
                FADM_Object.Communal._i_OptCupNum = 0;
                FADM_Object.Communal._i_optBottleNum = 0;
                int i_mRes = MyModbusFun.TargetMove(3, 0, 0);
                if (-2 == i_mRes)
                    throw new Exception("收到退出消息");


                FADM_Object.Communal._fadmSqlserver.InsertRun("RobotHand", "复位完成");
                //FADM_Object.Communal.WriteMachineStatus(0);
            }
            catch (Exception ex)
            {
                FADM_Object.Communal.WriteMachineStatus(8);
                FADM_Form.CustomMessageBox.Show(ex.Message, "MachineReset", MessageBoxButtons.OK, true);

            }
        }
        public static void MoveData(string s_batchName)
        {
            if (Convert.ToString(s_batchName) != "0")
            {
                if (FADM_Object.Communal._b_isDripAll)
                {
                    //添加历史表
                    DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                     "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                    string s_columnHead = null;
                    foreach (DataRow row in dt_temp.Rows)
                    {
                        string s_curName = Convert.ToString(row[0]);
                        if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                            "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                            s_columnHead += s_curName + ", ";
                    }
                    s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                    string s_columnDetails = null;
                    foreach (DataRow row in dt_temp.Rows)
                    {
                        string s_curName = Convert.ToString(row[0]);
                        if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName)
                            s_columnDetails += Convert.ToString(row[0]) + ", ";
                    }
                    s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM drop_head WHERE BatchName = '" + s_batchName + "'  And CupFinish = 0   ORDER BY CupNum;");

                    foreach (DataRow dataRow in dt_temp.Rows)
                    {
                        //先删除已有记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "DELETE FROM history_head WHERE BatchName = '" + s_batchName + "' AND CupNum = " +dataRow["CupNum"].ToString() + ";");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM history_details WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(dataRow["CupNum"].ToString())))
                        {
                            //滴液记录重新添加到批次
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "Update drop_head set BatchName = '0',Step=0,RealAddWaterWeight=0.0,AddWaterFinish=0,CupFinish=0 WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "Update  drop_details set BatchName = '0',Finish=0,RealDropWeight=0.00,MinWeight=0 WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");

                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "UPDATE cup_details SET FormulaCode = null, " +
                           "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                           "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                           "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                           dataRow["CupNum"].ToString() + "  ;");
                        }
                        else if (SmartDyeing.FADM_Object.Communal._lis_dyeCupNum.Contains(Convert.ToInt32(dataRow["CupNum"].ToString())))
                        {
                            DataTable dt_cup_details = FADM_Object.Communal._fadmSqlserver.GetData(
                                "SELECT * FROM cup_details WHERE CupNum = " + dataRow["CupNum"].ToString() + ";");
                            string sStatues = dt_cup_details.Rows[0]["Statues"].ToString();
                            if (sStatues != "下线")
                            {
                                if (sStatues != "待机")
                                {
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "DELETE FROM drop_head WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "' ;");
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //    "DELETE FROM drop_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");
                                    //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    //   "DELETE FROM dye_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';"); ;
                                    //Txt.DeleteTXT(i_cupNum);
                                    //Txt.DeleteMarkTXT(i_cupNum);
                                    FADM_Object.Communal._lis_dripStopCup.Add(Convert.ToInt32(dataRow["CupNum"].ToString()));
                                }
                                else
                                {
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "DELETE FROM drop_head WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "' ;");
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        "DELETE FROM drop_details WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "DELETE FROM dye_details WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");
                                    Txt.DeleteTXT(Convert.ToInt32(dataRow["CupNum"].ToString()));
                                    Txt.DeleteMarkTXT(Convert.ToInt32(dataRow["CupNum"].ToString()));
                                    FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "UPDATE cup_details SET FormulaCode = null, " +
                                   "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                   "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                   "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                   dataRow["CupNum"].ToString() + "  ;");

                                }
                            }
                        }
                    }

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                      "SELECT * FROM drop_head WHERE BatchName = '" + s_batchName + "'  And CupFinish = 1 And Stage = '滴液'   ORDER BY CupNum;");
                    foreach (DataRow dataRow in dt_temp.Rows)
                    {
                        //先删除已有记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "DELETE FROM history_head WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM history_details WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        //原来删除记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "DELETE FROM drop_head WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "' ;");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM drop_details WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");

                        //复位当前杯使用状态
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "UPDATE cup_details SET FormulaCode = null, " +
                            "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                            "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                            "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + dataRow["CupNum"].ToString() + " AND Statues != '下线';");
                    }
                }
                else
                {
                    for (int i = 1; i < 7; i++)
                    {
                        int i_cupMin = 0, i_cupMax = 0, i_type = 0;
                        switch (i)
                        {
                            case 1:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area1_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area1_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area1_Type;
                                break;
                            case 2:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area2_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area2_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area2_Type;
                                break;
                            case 3:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area3_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area3_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area3_Type;
                                break;
                            case 4:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area4_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area4_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area4_Type;
                                break;
                            case 5:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area5_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area5_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area5_Type;
                                break;
                            case 6:
                                i_cupMin = Lib_Card.Configure.Parameter.Machine_Area6_CupMin;
                                i_cupMax = Lib_Card.Configure.Parameter.Machine_Area6_CupMax;
                                i_type = Lib_Card.Configure.Parameter.Machine_Area6_Type;
                                break;
                            default:
                                break;

                        }



                        //添加历史表
                        DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                         "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_head';");
                        string s_columnHead = null;
                        foreach (DataRow row in dt_temp.Rows)
                        {
                            string s_curName = Convert.ToString(row[0]);
                            if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                                "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                                s_columnHead += s_curName + ", ";
                        }
                        s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                        dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                           "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'drop_details';");
                        string s_columnDetails = null;
                        foreach (DataRow row in dt_temp.Rows)
                        {
                            string s_curName = Convert.ToString(row[0]);
                            if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName)
                                s_columnDetails += Convert.ToString(row[0]) + ", ";
                        }
                        s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                        //先删除已有记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "DELETE FROM history_head WHERE BatchName = '" + s_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ";");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM history_details WHERE BatchName = '" + s_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            "WHERE BatchName = '" + s_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ");");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM drop_details " +
                           "WHERE BatchName = '" + s_batchName + "' AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + ");");




                        dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                            "SELECT * FROM drop_head WHERE BatchName = '" + s_batchName + "'  AND CupNum >= " + i_cupMin + " AND CupNum <= " + i_cupMax + "     And CupFinish = 0 ORDER BY CupNum;");
                        if (dt_temp.Rows.Count == 0)
                            continue;

                        foreach (DataRow dataRow in dt_temp.Rows)
                        {
                            int i_cupNum = Convert.ToInt16(dataRow["CupNum"]);
                            if (2 == i_type)
                            {
                                //原来删除记录
                                // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //        "DELETE FROM drop_head WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "' ;");
                                // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //     "DELETE FROM drop_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");

                                // FADM_Object.Communal._fadmSqlserver.ReviseData(
                                //"UPDATE cup_details SET FormulaCode = null, " +
                                //"DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                //"StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                //"TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                //i_cupNum + "  ;");

                                //滴液记录重新添加到批次
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "Update drop_head set BatchName = '0',Step=0,RealAddWaterWeight=0.0,AddWaterFinish=0,CupFinish=0 WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "' ;");
                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                                    "Update  drop_details set BatchName = '0',Finish=0,RealDropWeight=0.00,MinWeight=0 WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");

                                FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "UPDATE cup_details SET FormulaCode = null, " +
                               "DyeingCode = null, IsUsing = 1, Statues = '待机', " +
                               "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                               "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                               i_cupNum + "  ;");

                            }
                            else if (3 == i_type)
                            {
                                DataTable dt_cup_details = FADM_Object.Communal._fadmSqlserver.GetData(
                                    "SELECT * FROM cup_details WHERE CupNum = " + i_cupNum + ";");
                                string s_statues = dt_cup_details.Rows[0]["Statues"].ToString();
                                if (s_statues != "下线")
                                {
                                    if (s_statues != "待机")
                                    {
                                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //    "DELETE FROM drop_head WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "' ;");
                                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //    "DELETE FROM drop_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");
                                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                                        //   "DELETE FROM dye_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';"); ;
                                        //Txt.DeleteTXT(i_cupNum);
                                        //Txt.DeleteMarkTXT(i_cupNum);
                                        FADM_Object.Communal._lis_dripStopCup.Add(i_cupNum);
                                    }
                                    else
                                    {
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            "DELETE FROM drop_head WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "' ;");
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                            "DELETE FROM drop_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                           "DELETE FROM dye_details WHERE CupNum = " + i_cupNum + "  AND BatchName = '" + s_batchName + "';");
                                        Txt.DeleteTXT(i_cupNum);
                                        Txt.DeleteMarkTXT(i_cupNum);
                                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                                       "UPDATE cup_details SET FormulaCode = null, " +
                                       "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                                       "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                                       "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                                       i_cupNum + "  ;");

                                    }
                                }
                            }

                        }

                    }
                }
            }
        }

        public static void MoveData_ABS(string s_batchName)
        {
            if (Convert.ToString(s_batchName) != "0")
            {
                //if (FADM_Object.Communal._b_isDripAll)
                {
                    //添加历史表
                    DataTable dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                     "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_drop_head';");
                    string s_columnHead = null;
                    foreach (DataRow row in dt_temp.Rows)
                    {
                        string s_curName = Convert.ToString(row[0]);
                        if ("TestTubeFinish" != s_curName && "TestTubeWaterLower" != s_curName && "AddWaterFinish" != s_curName &&
                            "CupFinish" != s_curName && "TestTubeWaterLower" != s_curName)
                            s_columnHead += s_curName + ", ";
                    }
                    s_columnHead = s_columnHead.Remove(s_columnHead.Length - 2);

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = 'abs_drop_details';");
                    string s_columnDetails = null;
                    foreach (DataRow row in dt_temp.Rows)
                    {
                        string s_curName = Convert.ToString(row[0]);
                        if ("MinWeight" != s_curName && "Finish" != s_curName && "IsShow" != s_curName)
                            s_columnDetails += Convert.ToString(row[0]) + ", ";
                    }
                    s_columnDetails = s_columnDetails.Remove(s_columnDetails.Length - 2);

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                       "SELECT * FROM abs_drop_head WHERE BatchName = '" + s_batchName + "'  And CupFinish = 0   ORDER BY CupNum;");

                    foreach (DataRow dataRow in dt_temp.Rows)
                    {
                        //先删除已有记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "DELETE FROM abs_history_head WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM abs_history_details WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM abs_drop_head " +
                            "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO abs_history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM abs_drop_details " +
                           "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        //if (SmartDyeing.FADM_Object.Communal._lis_dripCupNum.Contains(Convert.ToInt32(dataRow["CupNum"].ToString())))
                        {
                            //滴液记录重新添加到批次
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                   "Update abs_drop_head set BatchName = '0',Step=0,RealAddWaterWeight=0.0,AddWaterFinish=0,CupFinish=0 WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "' ;");
                            FADM_Object.Communal._fadmSqlserver.ReviseData(
                                "Update  abs_drop_details set BatchName = '0',Finish=0,RealDropWeight=0.00,MinWeight=0 WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");

                           // FADM_Object.Communal._fadmSqlserver.ReviseData(
                           //"UPDATE cup_details SET FormulaCode = null, " +
                           //"DyeingCode = null, IsUsing = 1, Statues = '待机', " +
                           //"StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                           //"TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0 WHERE CupNum = " +
                           //dataRow["CupNum"].ToString() + "  ;");
                        }
                    }

                    dt_temp = FADM_Object.Communal._fadmSqlserver.GetData(
                      "SELECT * FROM abs_drop_head WHERE BatchName = '" + s_batchName + "'  And CupFinish = 1 And Stage = '滴液'   ORDER BY CupNum;");
                    foreach (DataRow dataRow in dt_temp.Rows)
                    {
                        //先删除已有记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "DELETE FROM abs_history_head WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM abs_history_details WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "INSERT INTO abs_history_head (" + s_columnHead + ") (SELECT " + s_columnHead + " FROM drop_head " +
                            "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                           "INSERT INTO history_details (" + s_columnDetails + ") (SELECT " + s_columnDetails + " FROM abs_drop_details " +
                           "WHERE BatchName = '" + s_batchName + "' AND CupNum = " + dataRow["CupNum"].ToString() + ";");

                        //原来删除记录
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                               "DELETE FROM abs_drop_head WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "' ;");
                        FADM_Object.Communal._fadmSqlserver.ReviseData(
                            "DELETE FROM abs_drop_details WHERE CupNum = " + dataRow["CupNum"].ToString() + "  AND BatchName = '" + s_batchName + "';");

                        ////复位当前杯使用状态
                        //FADM_Object.Communal._fadmSqlserver.ReviseData(
                        //    "UPDATE cup_details SET FormulaCode = null, " +
                        //    "DyeingCode = null, IsUsing = 0, Statues = '待机', " +
                        //    "StartTime = null, SetTemp = null, StepNum = null, TotalWeight = null, " +
                        //    "TotalStep = null, TechnologyName = null, StepStartTime = null, SetTime = null,RecordIndex = 0, Cooperate = 0 WHERE CupNum = " + dataRow["CupNum"].ToString() + " AND Statues != '下线';");
                    }
                }
                
            }
        }

        public static void IOReset()
        {
            Lib_Card.CardObject.OA1.SuddnStop(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_X);
            Lib_Card.CardObject.OA1.SuddnStop(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Y);
            Lib_Card.CardObject.OA1.SuddnStop(Lib_Card.ADT8940A1.ADT8940A1_IO.Axis_Z);
            int iRes = Lib_Card.CardObject.OA1Input.InPutStatus(Lib_Card.ADT8940A1.ADT8940A1_IO.InPut_Syringe);
            if (Lib_Card.Configure.Parameter.Machine_isSyringe == 1)
            {
                iRes = 0;
            }

            if (1 == iRes)
            {
                //new FADM_Object.MyAlarm("请先拿住针筒点确定", "温馨提示",false,1);
                FADM_Form.CustomMessageBox.Show("请先拿住针筒点确定", "温馨提示", MessageBoxButtons.OK, true);

                Lib_Card.ADT8940A1.OutPut.Tongs.Tongs tongs = new Lib_Card.ADT8940A1.OutPut.Tongs.Tongs_Basic();
                tongs.Tongs_Off();


            }

            for (int i = 0; i < 16; i++)
            {
                if (Lib_Card.Configure.Parameter.Machine_BlenderVersion == 0)
                {
                    if (i == ADT8940A1_IO.OutPut_Blender)
                    {
                        continue;
                    }
                }
                Lib_Card.CardObject.OA1.WriteOutPut(i, 0);
            }
        }

    }
}
