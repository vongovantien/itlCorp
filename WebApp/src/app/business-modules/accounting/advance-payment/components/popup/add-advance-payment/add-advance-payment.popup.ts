import { Component, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { AccoutingRepo } from 'src/app/shared/repositories';
import { catchError, map } from 'rxjs/operators';
import { CustomDeclaration } from 'src/app/shared/models';
import { ConfirmPopupComponent } from 'src/app/shared/common/popup';

@Component({
    selector: 'adv-payment-add-popup',
    templateUrl: './add-advance-payment.popup.html'
})

export class AdvancePaymentAddPopupComponent extends PopupBase {

    @ViewChild(ConfirmPopupComponent, {static: false}) confirmPopup: ConfirmPopupComponent;

    configShipment: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    selectedShipment: any = {};
    selectedShipmentData: OperationInteface.IShipment;

    types: CommonInterface.ICommonTitleValue[];
    selectedType: CommonInterface.ICommonTitleValue;

    customDeclarations: CustomDeclaration[];
    initCD: CustomDeclaration[];
    shipments: OperationInteface.IShipment[];
 
    form: FormGroup;
    description: AbstractControl;
    amount: AbstractControl;
    currency: AbstractControl;
    type: AbstractControl;
    note: AbstractControl;
    shipment: AbstractControl;
    customNo: AbstractControl;

    constructor(
        private _fb: FormBuilder,
        private _accoutingRepo: AccoutingRepo
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

        this.type.setValue(this.types[0]);
    }

    initForm() {
        this.form = this._fb.group({
            'description': [, Validators.compose([
                Validators.pattern(/^[\w '_"/*\\\.,-]*$/)
            ])],
            'amount': [],
            'note': [],
            'customNo': [],
            'shipment': [],
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

    onSubmit() {
        console.log(this.form.value);
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
            (errors: any) => {},
            () => {}
        );
    }

    getCustomNo() {
        this._accoutingRepo.getListCustomsDeclaration()
            .pipe(
                catchError(this.catchError),
                map((response: any[]) => response.map( (item: CustomDeclaration) => new CustomDeclaration(item))),
            )
            .subscribe(
                (res: any) => {
                    this.customDeclarations = this.initCD =  res || [];
                },
                (errors: any) => { },
                () => { },
            );
    }

    onSelectDataFormInfo(data: any , key: string) {
        switch (key) {
            case 'shipment':
                this.selectedShipment = { field: data.jobId, value: data.hbl };
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
        this.confirmPopup.show();
    }

    onSubmitExit() {
        this.confirmPopup.hide();
        this.hide();
    }
}

