import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'form-create-chart-of-accounts-popup',
    templateUrl: 'form-create-chart-of-accounts.popup.html'
})

export class FormCreateChartOfAccountsPopupComponent extends PopupBase implements OnInit {
    @Output() onChange: EventEmitter<boolean> = new EventEmitter<boolean>();

    formChart: FormGroup;

    isSubmitted: boolean = false;
    isUpdate: boolean = false;
    constructor(private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService) {
        super();
    }

    ngOnInit() {
        this.formChart = this._fb.group({
            accountCode: [null, Validators.required],
            accountNameLocal: [null, Validators.required],
            accountNameEn: [null, Validators.required],
            active: [true],
        });
    }
}