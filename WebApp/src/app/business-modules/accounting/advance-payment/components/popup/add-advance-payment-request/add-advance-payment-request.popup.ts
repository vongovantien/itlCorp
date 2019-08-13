import { Component, ViewChild, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, map } from 'rxjs/operators';
import { CustomDeclaration, AdvancePaymentRequest } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'adv-payment-add-popup',
    templateUrl: './add-advance-payment-request.popup.html'
})

export class AdvancePaymentAddRequestPopupComponent extends PopupBase {

    @Output() onRequest: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild(ConfirmPopupComponent, { static: false }) confirmPopup: ConfirmPopupComponent;

    action: string = 'create';

    configShipment: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    selectedShipment: any = {};
    selectedShipmentData: OperationInteface.IShipment = null;

    types: CommonInterface.ICommonTitleValue[];
    selectedType: CommonInterface.ICommonTitleValue;

    customDeclarations: CustomDeclaration[];
    initCD: CustomDeclaration[];

    form: FormGroup;
    description: AbstractControl;
    amount: AbstractControl;
    currency: AbstractControl;
    type: AbstractControl;
    note: AbstractControl;
    customNo: AbstractControl;

    bodyConfirm: string = 'Do you want to exit ?';

    selectedRequest: AdvancePaymentRequest; // TODO detect form was changed when dupplicate
    isDupplicate: boolean = false;

    constructor(
        private _fb: FormBuilder,
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.initBasicData();
        this.getListShipment();
        this.getCustomNo();
    }

    initBasicData() {
        this.types = [
            { title: 'Norm', value: 'Norm' },
            { title: 'Invoice', value: 'Invoice' },
            { title: 'Other', value: 'other' },
        ];

        this.type.setValue(this.types[2]);
    }

    initForm() {
        this.form = this._fb.group({
            'description': [, Validators.compose([
                Validators.pattern(/^[\w '_"/*\\\.,-]*$/),
                Validators.required
            ])],
            'amount': [,
                Validators.compose([
                    Validators.required,
                ])
            ],
            'note': [],
            'customNo': [],
            'type': [],
            'currency': [],
        });

        this.description = this.form.controls['description'];
        this.amount = this.form.controls['amount'];
        this.note = this.form.controls['note'];
        this.customNo = this.form.controls['customNo'];
        this.type = this.form.controls['type'];
        this.currency = this.form.controls['currency'];



    }

    initFormUpdate(data: AdvancePaymentRequest) {
        this.form.setValue({
            description: data.description,
            amount: data.amount,
            note: data.requestNote,
            customNo: !!data.customNo ? this.initCD.filter((item: CustomDeclaration) => item.clearanceNo === data.customNo)[0] : null,
            type: this.types.filter((type: any) => type.value === data.advanceType)[0],
            currency: data.requestCurrency
        });

        this.selectedShipmentData = <OperationInteface.IShipment>{ hbl: data.hbl, jobId: data.jobId, mbl: data.mbl };
        this.selectedShipment = { field: 'jobId', value: data.jobId };

        this.customDeclarations = [];
        this.customNo.setValue(null);

        this.customDeclarations = this.filterCDByShipment(this.selectedShipmentData);
        if (this.customDeclarations.length === 1) {
            this.customNo.setValue(this.customDeclarations[0]);
        }
    }

    onSubmit(form: FormGroup) {
        const body: AdvancePaymentRequest = new AdvancePaymentRequest({
            customNo: !!form.value.customNo ? form.value.customNo.clearanceNo : '',
            amount: form.value.amount,
            requestNote: form.value.note,
            hbl: this.selectedShipmentData.hbl,
            mbl: this.selectedShipmentData.mbl,
            jobId: this.selectedShipmentData.jobId,
            advanceType: form.value.type.value,
            requestCurrency: form.value.currency,
            description: form.value.description,
        });
        if (this.action === 'create') {
            this._accoutingRepo.checkShipmentsExistInAdvancePament(this.selectedShipmentData)
                .pipe(
                    catchError(this.catchError)
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (!res.status) {
                            this.onRequest.emit(body);
                            this.hide();
                            this.resetForm();
                        } else {
                            this._toastService.warning('Add New Advance Request Failed, please recheck !', 'Warning', { positionClass: 'toast-bottom-right' });
                        }
                    },
                    (errors: any) => { },
                    () => { }
                );
        } else {
            console.log(this.detectRequestChange(this.selectedRequest, body));
            if (this.detectRequestChange(this.selectedRequest, body)) {
                this.isDupplicate = true;
                this.bodyConfirm = 'Data already exists, do you want to save or not ?';
                this.confirmPopup.show();
            } else {
                this.isDupplicate = false;
                this.onRequest.emit(body);
                this.hide();
            }
        }
    }

    getListShipment() {
        this._accoutingRepo.getListShipmentDocumentOperation()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: OperationInteface.IShipment) => {
                    this.configShipment.dataSource = <any>res || [];

                    // * update config combogrid.
                    this.configShipment.displayFields = [
                        { field: 'jobId', label: 'Job No' },
                        { field: 'mbl', label: 'MBL' },
                        { field: 'hbl', label: 'HBL' },
                    ];
                    this.configShipment.selectedDisplayFields = ['jobId', `mbl`, 'hbl'];
                },
                (errors: any) => { },
                () => { }
            );
    }

    getCustomNo() {
        this._accoutingRepo.getListCustomsDeclaration()
            .pipe(
                catchError(this.catchError),
                map((response: any[]) => response.map((item: CustomDeclaration) => new CustomDeclaration(item))),
            )
            .subscribe(
                (res: any) => {
                    this.customDeclarations = this.initCD = res || [];
                },
                (errors: any) => { },
                () => { },
            );
    }

    onSelectDataFormInfo(data: any, key: string) {
        switch (key) {
            case 'shipment':
                this.selectedShipment = { field: 'jobId', value: data.hbl };
                this.selectedShipmentData = data;

                this.customDeclarations = [];
                this.customNo.setValue(null);

                this.customDeclarations = this.filterCDByShipment(this.selectedShipmentData);
                if (this.customDeclarations.length === 1) {
                    this.customNo.setValue(this.customDeclarations[0]);
                }
                break;

            default:
                break;
        }

    }

    filterCDByShipment(shipment: OperationInteface.IShipment): CustomDeclaration[] {
        return this.initCD.filter((item: CustomDeclaration) => {
            return (item.hblid === shipment.hbl && item.mblid === shipment.mbl && item.jobNo === shipment.jobId);
        });
    }

    onCancel() {
        this.bodyConfirm = 'Do you want to exit ?';
        this.confirmPopup.show();
    }

    resetForm() {
        this.description.setValue(null);
        this.note.setValue(null);
        this.amount.setValue(null);
        this.type.setValue(this.types[2]);
        this.customNo.setValue(null);

        this.description.markAsPristine({ onlySelf: true });
        this.description.markAsUntouched({ onlySelf: true });

        this.selectedShipment = {};
        this.selectedShipmentData = null;
    }

    onComfirmSaveDupplicateRequestAdvancePayment(form: FormGroup) {
        console.log("Add data in dupplicating");
        const body: AdvancePaymentRequest = new AdvancePaymentRequest({
            customNo: !!form.value.customNo ? form.value.customNo.clearanceNo : '',
            amount: form.value.amount,
            requestNote: form.value.note,
            hbl: this.selectedShipmentData.hbl,
            mbl: this.selectedShipmentData.mbl,
            jobId: this.selectedShipmentData.jobId,
            advanceType: form.value.type.value,
            requestCurrency: form.value.currency,
            description: form.value.description,
        });
        this.onRequest.emit(body);
    }

    onSubmitExitPopup() {
        this.confirmPopup.hide();
        this.hide();
        if (this.isDupplicate) {
            this.onComfirmSaveDupplicateRequestAdvancePayment(this.form);
        }
        this.resetForm();
    }

    detectRequestChange(requestInit: AdvancePaymentRequest, data: AdvancePaymentRequest): boolean {
        return (
            requestInit.description === data.description
            && requestInit.amount === data.amount
            && requestInit.requestNote === data.requestNote
            && requestInit.jobId === data.jobId
            && requestInit.advanceType === data.advanceType
            && requestInit.customNo === data.customNo
        );
    }
}
