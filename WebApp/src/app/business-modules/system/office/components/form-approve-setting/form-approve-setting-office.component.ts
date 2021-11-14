import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { LockShipmentSetting, FlowSetting } from '@models';
import { ChargeConstants } from '@constants';
import { ActivatedRoute, Params } from '@angular/router';
import { SystemRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: "form-approve-setting-office",
    templateUrl: "./form-approve-setting-office.component.html",
})
export class OfficeFormApproveSettingComponent
    extends AppForm
    implements OnInit
{
    approvePayments: FlowSetting[] = [];
    unlockShipments: FlowSetting[] = [];
    accountReceivable: FlowSetting = new FlowSetting();

    settings: CommonInterface.ICommonTitleValue[] = [
        { title: "None", value: "None" },
        { title: "Auto", value: "Auto" },
        { title: "Approval", value: "Approval" },
    ];

    partners: CommonInterface.ICommonTitleValue[] = [
        { title: "None", value: "None" },
        { title: "Customer", value: "Customer" },
        { title: "Agent", value: "Agent" },
        { title: "Both", value: "Both" },
    ];

    types: CommonInterface.ICommonTitleValue[] = [
        { title: "None", value: "None" },
        { title: "Alert", value: "Alert" },
        { title: "Check Point", value: "Check Point" },
    ];

    settingSpecials: CommonInterface.ICommonTitleValue[] = [
        ...this.settings,
        { title: "Special", value: "Special" },
    ];

    serviceLockSettings: LockShipmentSetting[] = [];
    dates: number[] = [
        1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
        21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
    ];
    services: string[] = [
        ChargeConstants.AE_CODE,
        ChargeConstants.AI_CODE,
        ChargeConstants.SLI_CODE,
        ChargeConstants.SLE_CODE,
        ChargeConstants.SFI_CODE,
        ChargeConstants.SFE_CODE,
        ChargeConstants.SCI_CODE,
        ChargeConstants.SCE_CODE,
        ChargeConstants.CL_CODE,
    ];
    officeId: string;

    constructor(
        private _activedRouter: ActivatedRoute,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.officeId = param.id;
                this.getSetting(this.officeId);
            }
        });
    }

    initUnlockShipmentSetting() {
        this.unlockShipments.push(
            ...[
                new FlowSetting({ type: "Shipment", flow: "Unlock" }),
                new FlowSetting({ type: "Advance", flow: "Unlock" }),
                new FlowSetting({ type: "Settlement", flow: "Unlock" }),
            ]
        );
    }

    initLockingShipmentSetting() {
        for (let index = 0; index < 9; index++) {
            this.serviceLockSettings.push(
                new LockShipmentSetting({ serviceType: this.services[index] })
            );
        }
    }

    initApprovalSetting() {
        this.approvePayments.push(
            ...[
                new FlowSetting({ type: "Advance", flow: "Approval" }),
                new FlowSetting({ type: "Settlement", flow: "Approval" }),
            ]
        );
    }

    getSetting(officeId: string) {
        this._systemRepo
            .getSettingFlowByOffice(officeId)
            .subscribe(
                (res: {
                    lockingDateShipment: LockShipmentSetting[];
                    approvals: FlowSetting[];
                    unlocks: FlowSetting[];
                    account: FlowSetting;
                }) => {
                    if (!res.lockingDateShipment.length) {
                        this.initLockingShipmentSetting();
                    } else {
                        this.serviceLockSettings = res.lockingDateShipment;
                    }

                    if (!res.approvals.length) {
                        this.initApprovalSetting();
                    } else {
                        this.approvePayments = res.approvals;
                    }

                    if (!res.unlocks.length) {
                        this.initUnlockShipmentSetting();
                    } else {
                        this.unlockShipments = res.unlocks;
                    }
                    if (!res.account) {
                        this.accountReceivable = new FlowSetting();
                        this.accountReceivable.type = "AccountReceivable";
                    } else {
                        this.accountReceivable = res.account;
                    }
                    if(!res.account.applyPartner){
                        this.accountReceivable.applyPartner="None";
                    } 
                    if(!res.account.applyType){
                        this.accountReceivable.applyType="None";
                    } 
                }
            );
    }

    onSave() {
        this.isSubmitted = true;

        if (!this.checkValidate()) {
            this._toastService.warning(this.invalidFormText);
            return;
        }

        const body: ISettingFlowEditModel = {
            officeId: this.officeId,
            approvePayments: this.approvePayments,
            unlockShipments: this.unlockShipments,
            lockShipmentDate: this.serviceLockSettings,
            accountReceivable: this.accountReceivable,
        };

        this._systemRepo
            .updateSettingFlow(body)
            .subscribe((res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                }
            });
    }

    checkValidate() {
        let valid = true;

        for (const charge of this.serviceLockSettings) {
            if (!charge.lockDate || !charge.lockAfterUnlocking) {
                valid = false;
                break;
            }
        }
        return valid;
    }
}

interface ISettingFlowEditModel {
    officeId: string;
    approvePayments: FlowSetting[];
    unlockShipments: FlowSetting[];
    lockShipmentDate: LockShipmentSetting[];
    accountReceivable: FlowSetting;
}
