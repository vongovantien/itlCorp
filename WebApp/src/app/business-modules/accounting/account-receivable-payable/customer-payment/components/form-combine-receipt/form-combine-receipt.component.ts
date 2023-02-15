import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { formatDate } from '@angular/common';
import { Component, Input, OnInit, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Params } from '@angular/router';
import { ComboGridVirtualScrollComponent } from '@common';
import { JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { SelectPartnerReceiptCombine, UpdateExchangeRateReceiptCombine } from '../../store/actions';
import { ICustomerPaymentState } from '../../store/reducers';

type COMBINE_TYPE = 'NEW' | 'EXISTING';
@Component({
    selector: 'form-create-receipt-combine',
    templateUrl: './form-combine-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptCombineComponent extends AppForm implements OnInit {
    @ViewChild('comboGridPartner') combogridPartner: ComboGridVirtualScrollComponent;

    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    get readonly(): boolean {
        return this._readonly;
    }
    private _readonly: boolean = false;

    partnerId: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    currency: AbstractControl;
    combineNo: AbstractControl;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    partners: Observable<Partner[]>;
    partnerName: string;

    typeCombine: COMBINE_TYPE = 'NEW';

    exsitingCombines: Observable<any>;
    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _activedRouter: ActivatedRoute,
        private readonly _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this._activedRouter.data
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (paramData: Params) => {
                    console.log({ paramData });
                    this.typeCombine = paramData.type;
                }
            )
        this.initForm();

        this.partners = this._catalogueRepo.getPartnerByGroups([CommonEnum.PartnerGroupEnum.AGENT]);
        this._catalogueRepo.convertExchangeRate(formatDate(new Date(), 'yyyy-MM-dd', 'en'), 'USD')
            .subscribe(
                (value: {
                    id: number;
                    currencyFromID: string;
                    rate: number;
                    currencyToID: string;
                }) => {
                    this.exchangeRate.setValue(value.rate);
                    this._store.dispatch(UpdateExchangeRateReceiptCombine({ exchangeRate: value.rate }));
                }
            )

    }

    initForm() {
        this.form = this._fb.group({
            partnerId: [null, Validators.required],
            paymentDate: [new Date()],
            exchangeRate: [null],
            currency: [{ value: 'USD', disabled: true }],
            combineNo: [null, this.typeCombine === 'EXISTING' ? Validators.required : null]
        });
        this.partnerId = this.form.controls['partnerId'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.currency = this.form.controls['currency'];
        this.combineNo = this.form.controls['combineNo'];
        this.exchangeRate = this.form.controls['exchangeRate'];
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                const partner = (data as Partner);
                this._catalogueRepo.getAgreement(
                    <any>{
                        partnerId: partner.id,
                        status: true
                    }).subscribe(
                        (agreements: any[]) => {
                            console.log({ agreements });
                            if (!agreements.length) {
                                this.combogridPartner.displaySelectedStr = '';
                                this.partnerId.setValue(null);
                                this.partnerName = '';
                                this._toastService.warning(`Partner ${data.shortName} does not have any agreement`);
                                return;
                            }
                            this.partnerId.setValue(partner.id);
                            this.partnerName = partner.shortName;
                            // TODO Dispatch select partner combine.
                            this._store.dispatch(SelectPartnerReceiptCombine({
                                id:
                                    this.partnerId.value,
                                shortName: partner.shortName,
                                accountNo: partner.accountNo,
                                partnerNameEn: partner.partnerNameEn
                            }))
                        }
                    );
                break;
            default:
                break;
        }
    }

}
