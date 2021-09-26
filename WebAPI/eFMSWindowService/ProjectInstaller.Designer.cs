﻿namespace eFMSWindowService
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller2 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller3 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstaller4 = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstallerAutoLockShipment = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstallerSendMailOverduePayment = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstallerSendMailExceededCreditLimit = new System.ServiceProcess.ServiceInstaller();
            this.serviceInstallerSendMailexpiredAgreement = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "UpdateExchangeRate";
            this.serviceInstaller1.DisplayName = "eFMSWindowService.UpdateExchangeRate";
            this.serviceInstaller1.ServiceName = "UpdateExchangeRate";
            this.serviceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstaller2
            // 
            this.serviceInstaller2.Description = "UpdateCurrentStatusOfJobService";
            this.serviceInstaller2.DisplayName = "eFMSWindowService.UpdateCurrentStatusOfJobService";
            this.serviceInstaller2.ServiceName = "UpdateCurrentStatusOfJobService";
            this.serviceInstaller2.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstaller3
            // 
            this.serviceInstaller3.Description = "UpdateStatusAuthorization";
            this.serviceInstaller3.DisplayName = "eFMSWindowService.UpdateStatusAuthorization";
            this.serviceInstaller3.ServiceName = "UpdateStatusAuthorization";
            this.serviceInstaller3.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstaller4
            // 
            this.serviceInstaller4.Description = "SendMailToARDepartmentService";
            this.serviceInstaller4.DisplayName = "eFMSWindowService.SendMailToARDepartmentService";
            this.serviceInstaller4.ServiceName = "SendMailToARDepartmentService";
            this.serviceInstaller4.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstallerAutoLockShipment
            // 
            this.serviceInstallerAutoLockShipment.Description = "AutoLockShipmentService";
            this.serviceInstallerAutoLockShipment.DisplayName = "eFMSWindowService.AutoLockShipmentService";
            this.serviceInstallerAutoLockShipment.ServiceName = "AutoLockShipmentService";
            this.serviceInstallerAutoLockShipment.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstallerSendMailOverduePayment
            // 
            this.serviceInstallerSendMailOverduePayment.Description = "SendMailOverduePaymentService";
            this.serviceInstallerSendMailOverduePayment.DisplayName = "eFMSWindowService.SendMailOverduePaymentService";
            this.serviceInstallerSendMailOverduePayment.ServiceName = "SendMailOverduePaymentService";
            this.serviceInstallerSendMailOverduePayment.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstallerSendMailExceededCreditLimit
            // 
            this.serviceInstallerSendMailExceededCreditLimit.Description = "SendMailExceededCreditLimitService";
            this.serviceInstallerSendMailExceededCreditLimit.DisplayName = "eFMSWindowService.SendMailExceededCreditLimitService";
            this.serviceInstallerSendMailExceededCreditLimit.ServiceName = "SendMailExceededCreditLimitService";
            this.serviceInstallerSendMailExceededCreditLimit.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // serviceInstallerSendMailexpiredAgreement
            // 
            this.serviceInstallerSendMailexpiredAgreement.Description = "SendMailExpiredAgreementService";
            this.serviceInstallerSendMailexpiredAgreement.DisplayName = "eFMSWindowService.SendMailExpiredAgreementService";
            this.serviceInstallerSendMailexpiredAgreement.ServiceName = "SendMailExpiredAgreementService";
            this.serviceInstallerSendMailexpiredAgreement.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1,
            this.serviceInstaller2,
            this.serviceInstaller3,
            this.serviceInstaller4,
            this.serviceInstallerAutoLockShipment,
            this.serviceInstallerSendMailOverduePayment,
            this.serviceInstallerSendMailExceededCreditLimit,
            this.serviceInstallerSendMailexpiredAgreement});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller2;
        private System.ServiceProcess.ServiceInstaller serviceInstaller3;
        private System.ServiceProcess.ServiceInstaller serviceInstaller4;
        private System.ServiceProcess.ServiceInstaller serviceInstallerAutoLockShipment;
        private System.ServiceProcess.ServiceInstaller serviceInstallerSendMailOverduePayment;
        private System.ServiceProcess.ServiceInstaller serviceInstallerSendMailExceededCreditLimit;
        private System.ServiceProcess.ServiceInstaller serviceInstallerSendMailexpiredAgreement;
    }
}