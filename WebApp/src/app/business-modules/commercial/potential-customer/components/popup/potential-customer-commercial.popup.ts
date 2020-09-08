import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { FormBuilder, FormGroup, AbstractControl, Validators, FormControl } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { CatPotentialModel, PotentialUpdateModel } from '@models';
import { SystemConstants } from '@constants';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize } from 'rxjs/operators';

@Component({
    selector: 'potential-customer-commercial-popup',
    templateUrl: './potential-customer-commercial.popup.html',
})

export class CommercialPotentialCustomerPopupComponent extends PopupBase implements OnInit {

    @Output() onChange: EventEmitter<boolean> = new EventEmitter<boolean>();

    isSubmitted: boolean = false;
    isUpdate: boolean = false;
    //
    potentialTypes: CommonInterface.INg2Select[] = [
        { id: 'Company', text: 'Company' },
        { id: 'Invidual', text: 'Invidual' }
    ];
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
    potentialType: AbstractControl;
    active: AbstractControl;

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
        private _ngProgessSerice: NgProgress,
    ) {
        super();
        this._progressRef = this._ngProgessSerice.ref();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formPotential = this._fb.group({
            nameEn: [null, Validators.required],
            nameLocal: [],
            taxcode: [null, Validators.compose([
                Validators.maxLength(14),
                Validators.minLength(8),
                Validators.pattern(SystemConstants.CPATTERN.TAX_CODE),
                Validators.required
            ])
            ],
            tel: [],
            address: [],
            email: [],
            margin: [null, Validators.compose([
                Validators.min(0),
                Validators.max(100),
                Validators.pattern(SystemConstants.CPATTERN.NUMBER)
            ])],
            quotation: [null, Validators.compose([
                Validators.pattern(SystemConstants.CPATTERN.NUMBER)
            ])],
            potentialType: [this.potentialTypes[0]],
            active: [true],
        });
        this.nameEn = this.formPotential.controls['nameEn'];
        this.nameLocal = this.formPotential.controls['nameLocal'];
        this.taxcode = this.formPotential.controls['taxcode'];
        this.tel = this.formPotential.controls['tel'];
        this.address = this.formPotential.controls['address'];
        this.email = this.formPotential.controls['email'];
        this.margin = this.formPotential.controls['margin'];
        this.quotation = this.formPotential.controls['quotation'];
        this.potentialType = this.formPotential.controls['potentialType'];
        this.active = this.formPotential.controls['active'];
    }

    handleSubmit() {
        this.isSubmitted = true;
        const dataForm: { [key: string]: any } = this.formPotential.getRawValue();


        if (this.formPotential.valid) {
            const potential: CatPotentialModel = new CatPotentialModel({
                ...dataForm,
                potentialType: dataForm.potentialType.id,
            });
            const body: PotentialUpdateModel = {
                potential: potential,

            };



            if (this.isUpdate) {
                this._progressRef.start();
                this._catalogueRepo.updatePotential(body)
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => {
                            this._progressRef.complete();


                        })
                    )
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.handleResponseResult(res);
                        }
                    );
            } else {
                this._progressRef.start();
                this._catalogueRepo.createPotential(body)
                    .pipe(
                        catchError(this.catchError),
                        finalize(() => {
                            this._progressRef.complete();


                        })
                    )
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            this.handleResponseResult(res);
                        }
                    );
            }
        }

    }
    //
    handleResponseResult(res: CommonInterface.IResult) {
        if (res.status) {
            this._toastService.success(res.message);
            this.hide();
            this.onChange.emit(true);
        } else {
            this._toastService.error(res.message);
        }
    }
}


