import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormBuilder, FormGroup, AbstractControl, Validators, FormControl } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'potential-customer-commercial-popup',
    templateUrl: './potential-customer-commercial.popup.html',
})

export class CommercialPotentialCustomerPopupComponent extends PopupBase implements OnInit {

    isSubmitted: boolean = false;
    isUpdate: boolean = false;
    //form
    formPotential: FormGroup;
    //fields
    nameEn: AbstractControl;
    nameLocal: AbstractControl;
    taxcode: AbstractControl;
    tel: AbstractControl;
    address: AbstractControl;
    email: AbstractControl;
    margin: AbstractControl;
    quotation: AbstractControl;
    potentialType = null;
    /*potentialTypes: CommonInterface.INg2Select[] = [
        { id: 'Company', text: 'Company' },
        { id: 'Invidual', text: 'Invidual' }
    ];*/
    //potentialTypes: any[] = ['Company', 'Invidual']
    potentialTypes = [
        { value: 1, label: 'Vilnius' },
        { value: 2, label: 'Kaunas' },
        { value: 3, label: 'Pavilnys', disabled: true }
    ];
    active: AbstractControl;

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
    }

    ngOnInit() {

    }
    initForm() {
        this.formPotential = this._fb.group({

        })
    }

}


