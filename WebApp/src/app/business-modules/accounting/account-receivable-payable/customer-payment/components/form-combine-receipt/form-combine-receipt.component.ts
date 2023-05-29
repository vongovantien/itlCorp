import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { ComboGridVirtualScrollComponent, ConfirmPopupComponent } from '@common';
import { RoutingConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { combineLatest, Observable } from 'rxjs';
import { take, takeUntil } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { ResetCombineInvoiceList, ResetInvoiceList, SelectPartnerReceiptCombine, UpdateExchangeRateReceiptCombine } from '../../store/actions';
import { ICustomerPaymentState, ReceiptCombineCreditListState, ReceiptCombineGeneralListState } from '../../store/reducers';
import { InjectViewContainerRefDirective } from '@directives';

type COMBINE_TYPE = 'NEW' | 'EXISTING';
@Component({
    selector: 'form-create-receipt-combine',
    templateUrl: './form-combine-receipt.component.html',
})
export class ARCustomerPaymentFormCreateReceiptCombineComponent extends AppForm implements OnInit {
    @ViewChild('comboGridPartner') combogridPartner: ComboGridVirtualScrollComponent;
    @ViewChild(InjectViewContainerRefDirective) injectViewContainer: InjectViewContainerRefDirective;
    
    @Output() onSynceCombine: EventEmitter<any> = new EventEmitter<any>();
    @Input() isUpdate: boolean = false;
    isBalance: boolean = false;

    partnerId: AbstractControl;
    paymentDate: AbstractControl;
    exchangeRate: AbstractControl;
    currency: AbstractControl;
    combineNo: AbstractControl;

    arcbNo: string = null;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = <CommonInterface.IComboGridDisplayField[]>[
        { field: 'accountNo', label: 'Partner Code' },
        { field: 'shortName', label: 'Name ABBR' },
        { field: 'salemanName', label: 'Saleman Name' },
        { field: 'contractType', label: 'Contract Type' },
    ];
    selectedDisplayFieldAgent: ['shortName', 'salemanName']
    selectedAgentData : any = {};

    partners: Observable<Partner[]>;
    partnerName: string;
    isSubmitted: boolean = false;
    selectedPartner: any = {};
    isAllDone: boolean = false;

    typeCombine: string = 'new';

    exsitingCombines: Observable<any>;
    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _fb: FormBuilder,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _activedRouter: ActivatedRoute,
        private readonly _toastService: ToastrService,
        private readonly _actionStoreSubject: ActionsSubject,
        protected readonly _router: Router,
    ) {
        super();
    }

    ngOnInit(): void {
        this.partners = this.isUpdate ? null : this._catalogueRepo.getPartnerGroupsWithCriteria({ partnerGroups: [CommonEnum.PartnerGroupEnum.AGENT], partnerType : 'Agent', isShowSaleman: true});
        this._activedRouter.data
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (paramData: Params) => {
                    this.typeCombine = paramData.type;
                }
            )
        this.initForm();
        this.getExchangeRate(this.paymentDate.value?.startDate);
    }

    initForm() {
        this.form = this._fb.group({
            partnerId: [null, Validators.required],
            paymentDate: [{ startDate: new Date(), endDate: new Date() }],
            exchangeRate: [null],
            currency: [{ value: 'USD', disabled: true }],
            combineNo: [null, this.typeCombine === 'existing' ? Validators.required : null]
        });
        this.partnerId = this.form.controls['partnerId'];
        this.paymentDate = this.form.controls['paymentDate'];
        this.currency = this.form.controls['currency'];
        this.combineNo = this.form.controls['combineNo'];
        this.exchangeRate = this.form.controls['exchangeRate'];
    }

    getExchangeRate(date: any = null) {
        if (!!date && !this.isUpdate) {
            this._catalogueRepo.convertExchangeRate(formatDate(new Date(date), 'yyyy-MM-dd', 'en-US'), 'USD')
                .pipe(take(1))
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
                );
        }
    }

    onChangePaymentDate(date: any) {
        if(!this.isUpdate){
            this.getExchangeRate(date.startDate);
        }
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'partner':
                this.selectedPartner = data;
                this.partnerId.setValue(data.id);
                this._store.dispatch(ResetCombineInvoiceList());
                this._store.dispatch(ResetInvoiceList());
                // TODO Dispatch select partner combine.
                this._store.dispatch(SelectPartnerReceiptCombine({
                    id: data.id,
                    shortName: data.shortName,
                    accountNo: data.accountNo,
                    partnerNameEn: data.partnerNameEn,
                    salemanId: data.salemanId,
                    salemanName: data.salemanName,
                    contractId: data.contractId
                }))
                break;
                case 'exchangeRate':
                    if (!data.target.value.length) {
                        this.exchangeRate.setValue(0);
                    } 
                    this._store.dispatch(UpdateExchangeRateReceiptCombine({ exchangeRate: this.exchangeRate.value }));
                    break;
            default:
                break;
        }
    }

    gotoList() {
        this._store.dispatch(ResetCombineInvoiceList());
        this._store.dispatch(ResetInvoiceList());
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);

    }

    confirmCancel() {
        let dataList = [];
        combineLatest([
            this._store.select(ReceiptCombineCreditListState),
            this._store.select(ReceiptCombineCreditListState),
            this._store.select(ReceiptCombineGeneralListState)])
            .subscribe(x => {
                x.forEach((element: any) => {
                    if (!!element && element?.length > 0) {
                        element.map(item => dataList.push(item))
                    }
                });
            });

        if (dataList.length > 0) {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.injectViewContainer.viewContainerRef, {
                body: 'Do you want to exit without saving?',
            }, () => {
                this.gotoList();
            })
        } else {
            this.gotoList();
        }
    }

    confirmSync() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.injectViewContainer.viewContainerRef, {
            title: 'Sync To Accountant System',
            body: 'Are you sure to send these data to accountant system',
            iconConfirm: 'la la-cloud-upload',
            labelConfirm: 'Yes'
        }, () => {
            this.onSynceCombine.emit(true);
        });
    }
}
