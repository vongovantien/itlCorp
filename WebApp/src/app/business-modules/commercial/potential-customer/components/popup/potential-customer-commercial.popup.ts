import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormBuilder, FormGroup, AbstractControl, Validators } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'potential-customer-commercial-popup',
    templateUrl: './potential-customer-commercial.popup.html'
})

export class CommercialPotentialCustomerPopupComponent extends PopupBase implements OnInit {

    isUpdate: boolean = false;
    constructor(

    ) {
        super();
    }

    ngOnInit() {

    }


}


