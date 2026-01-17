using Microsoft.Ajax.Utilities;
using WebApplicationUnipi.Models.Enums;
using WebApplicationUnipi.Models.MyApplicationUnipi;
using WebApplicationUnipi.Services.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models.AnnualLeaves
{
    public class CustomLeaves
    {
        public int Id { get; set; }
        public Nullable<int> AnnualLeavesId { get; set; }        
        public Nullable<int> Employee_Id { get; set; }
        public Nullable<short> Year { get; set; }
        public Nullable<byte> AnnualSum { get; set; }

        public int LeaveType { get; set; }
        public Nullable<byte> Available { get; set; }
        public Nullable<byte> Status { get; set; }
        public Nullable<int> ContractStatus { get; set; }
        public string contractInfo { get; set; }


        public CustomLeaves()
        {
        }

        public CustomLeaves(AnnualLeavesContractsView dbObj)
        {
            Id = dbObj.Id;
            AnnualLeavesId = dbObj.AnnualLeavesId;
            Employee_Id = dbObj.Employee_Id;
            Year = dbObj.Year;
            AnnualSum = dbObj.AnnualSum;           
            Available = dbObj.Available;
            LeaveType = (int)dbObj.LeaveType;
            Status = dbObj.Status;

            var db = new MyApplicationUnipiEntities();

            string sDate = (dbObj.Start_Date != null) ? dbObj.Start_Date.Value.Date.ToString("dd/MM/yy") : "";
            string eDate = (dbObj.End_Date != null) ? dbObj.End_Date.Value.Date.ToString("dd/MM/yy") : "...";
            string descr = (dbObj.Description != null) ? ((Enums.Contracts)dbObj.Description).GetDisplayAttributeFrom() : "";
            //string descr = (dbObj.Contract_Type_Id != null) ? db.Contract_Types.FirstOrDefault(f => f.Id == dbObj.Contract_Type_Id).Name : "";
            string stat = ((Enums.ContractsStatus)dbObj.ContractStatus).GetDisplayAttributeFrom() ?? "";
            contractInfo = string.Format("{0} - {1} - {2} - {3}", descr, sDate , eDate, stat);
        }
    }
}