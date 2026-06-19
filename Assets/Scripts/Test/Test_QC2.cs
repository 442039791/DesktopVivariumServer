using UnityEngine;
using UnityEngine.UI;
using SUIFW;
public class Test_QC2 : BaseUIForms
{
    public Button Testbutton;
    public int area_x;
    public int area_y;
    public int area_z;
    public Vector3 Pos;
    public string PlantType;

    public Button TestcreateFish;
    public Button TestCreatePlant;
    public Button TestShowWarehouse;
    public Button TestShowBioTankInfo;
    private void Start()
    {
        Testbutton.AddDeskTopListener(() =>
        {
            //BioTankStateData bioTankStateData = new()
            //{
            //    area_x = 22,
            //    area_y = 17,
            //    area_z = 16,
            //    Pos = Pos,
            //    ID = 0
            //};
            //PlayerManager.Instance.CreateBioTank(bioTankStateData,true);
        });
        TestcreateFish.AddDeskTopListener(() =>
        {
            AnimalStateData fishStateData = new()
            {
                ID = 0,
                //JuvenileNum = 1,
                //AdultNum = 2,
                Position = new Vector3(Pos.x + area_x / 2, Pos.y, Pos.z),
                Type = "101"
            };
            PlayerManager.Instance.CreateAnimal(fishStateData);
        });
        TestCreatePlant.AddDeskTopListener(() =>
        {
            PlantStateData plantStateData = new()
            {
                ID = 0,
                Type = PlantType,
                AsDeath = false,
                GrowthProgress = 1f
            };
            //PlayerManager.Instance.CreatePlantOnCube(plantStateData);
        });
        //TestShowWarehouse.AddDeskTopListener(() =>
        //{
        //    WarehouseManager.Instance.uI_Warehouse=(UI_Warehouse)UIManager.Instance.ShowUIForms(SysDefine.UI_Warehouse);
        //});
        TestShowBioTankInfo.AddDeskTopListener(() =>
        {
            //PlayerManager.Instance.actionBioTankManager.uIInfo= (UI_BioTankInfo)UIManager.Instance.ShowUIForms(SysDefine.UI_BioTankInfo);

        });
    }
}
//public int area_x;
//// 面积的高度（y方向）
//public int area_y;
//// 面积的深度（z方向）
//public int area_z;
//public Vector3 Pos;
//public int ID;