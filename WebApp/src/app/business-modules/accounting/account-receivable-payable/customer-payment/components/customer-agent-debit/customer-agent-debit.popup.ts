import { Component } from '@angular/core';
import { PopupBase } from '@app';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { Observable } from 'rxjs';
import { Customer } from '@models';
import { CatalogueRepo } from '@repositories';
import { CommonEnum } from '@enums';
import { JobConstants, ChargeConstants } from '@constants';

@Component({
    selector: 'customer-agent-debit-popup',
    templateUrl: 'customer-agent-debit.popup.html'
})

export class CustomerAgentDebitPopupComponent extends PopupBase {

    typeSearch: AbstractControl;
    partnerName: AbstractControl;
    referenceNo: AbstractControl;
    date: AbstractControl;
    dateType: AbstractControl;
    service: AbstractControl;

    type: string = '';
    formSearch: FormGroup;
    searchOptions: string[] = ['Soa', 'Debit Note/Invoice', 'VAT Invoice', 'Job No', 'HBL', 'MBL', 'Customs No'];
    customers: Observable<Customer[]>;
    displayFilesPartners: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    dateTypeList: string[] = ['Invoice Date', 'Service Date', 'Billing Date'];

    services: CommonInterface.INg2Select[] = [
        { text: ChargeConstants.IT_DES, id: ChargeConstants.IT_CODE },
        { text: ChargeConstants.AI_DES, id: ChargeConstants.AI_CODE },
        { text: ChargeConstants.AE_DES, id: ChargeConstants.AE_CODE },
        { text: ChargeConstants.SFE_DES, id: ChargeConstants.SFE_CODE },
        { text: ChargeConstants.SFI_DES, id: ChargeConstants.SFI_CODE },
        { text: ChargeConstants.SLE_DES, id: ChargeConstants.SLE_CODE },
        { text: ChargeConstants.SLI_DES, id: ChargeConstants.SLI_CODE },
        { text: ChargeConstants.SCE_DES, id: ChargeConstants.SCE_CODE },
        { text: ChargeConstants.SCI_DES, id: ChargeConstants.SCI_CODE },
        { text: ChargeConstants.CL_DES, id: ChargeConstants.CL_CODE }
    ];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo

    ) {
        super();
    }


    ngOnInit() {
        this.initForm();
        this.getCustomer();
    }
    initForm() {
        this.formSearch = this._fb.group({
            typeSearch: [],
            referenceNo: [],
            partnerName: [],
            date: [],
            dateType: [],
            service: []
        });
        this.typeSearch = this.formSearch.controls['typeSearch'].value;
        this.referenceNo = this.formSearch.controls['referenceNo'].value;
        this.date = this.formSearch.controls['date'].value;
        this.dateType = this.formSearch.controls['dateType'].value;
        this.service = this.formSearch.controls['service'].value;

    }

    getCustomer() {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
    }

    onSelectDataFormInfo($event: any) {
        console.log($event);
    }

    onApply() {

    }

    reset() {

    }

}