import { formatDate } from '@angular/common';
import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppForm } from '@app';
import { ConfirmPopupComponent } from '@common';
import { AccountingConstants, RoutingConstants, SystemConstants } from '@constants';
import { IReceiptCombineGroup, ReceiptInvoiceModel, ReceiptModel } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { AccountingRepo } from '@repositories';
import { IAppState } from '@store';
import groupBy from 'lodash/groupBy';
import { ToastrService } from 'ngx-toastr';
import { EMPTY, combineLatest, of } from 'rxjs';
import { concatMap, filter, map, take, takeUntil } from 'rxjs/operators';
import { ARCustomerPaymentReceiptCDCombineComponent } from '../components/cd-combine/receipt-cd-combine.component';
import { ARCustomerPaymentFormCreateReceiptCombineComponent } from '../components/form-combine-receipt/form-combine-receipt.component';
import { ARCustomerPaymentReceiptGeneralCombineComponent } from '../components/general-combine/receipt-general-combine.component';
import { AddCreditCombineToReceipt, AddDebitCombineToReceipt, IsCombineReceipt, ReceiptCombineActionTypes, RegistTypeReceipt, ResetCombineInvoiceList, ResetInvoiceList, SelectedAgreementReceiptCombine, SelectPartnerReceiptCombine, UpdateExchangeRateReceiptCombine } from '../store/actions';
import { ReceiptCombineAgreementState, ReceiptCombineCreditListState, ReceiptCombineDeditListState, ReceiptCombineGeneralListState } from '../store/reducers';
import { HttpErrorResponse } from '@angular/common/http';
export enum SaveReceiptActionEnum {
    DRAFT_CREATE = 0,
    DRAFT_UPDATE = 1,
    DONE = 2,
    CANCEL = 3,
}

@Component({
    selector: 'app-create-receip-combine',
    templateUrl: './create-receipt-combine.component.html',
})
export class ARCustomerPaymentCreateReciptCombineComponent  extends AppForm implements OnInit {
    @ViewChild(ARCustomerPaymentFormCreateReceiptCombineComponent) CreateReceiptCombineComponent: ARCustomerPaymentFormCreateReceiptCombineComponent;
    @ViewChild(ARCustomerPaymentReceiptGeneralCombineComponent) ReceiptGeneralCombineComponent: ARCustomerPaymentReceiptGeneralCombineComponent;
    @ViewChild('CreditPayment') CreditPaymentReceiptCDCombineComponent: ARCustomerPaymentReceiptCDCombineComponent;
    @ViewChild('DebitPayment') DebitPaymentReceiptCDCombineComponent: ARCustomerPaymentReceiptCDCombineComponent;

    @Output() onAddCreditCombine: EventEmitter<any> = new EventEmitter<any>();

    generalList$ = this._store.select(ReceiptCombineGeneralListState);
    debitList$ = this._store.select(ReceiptCombineDeditListState);
    creditList$ = this._store.select(ReceiptCombineCreditListState);
    receiptCreditGroups: any[] = [];

    receiptDebitGroups: any[] = [];
    formCreateMapValue: any = {};
    conbineType: string = '';

    constructor(protected readonly _store: Store<IAppState>,
        protected readonly _accountingRepo: AccountingRepo,
        protected readonly _router: Router,
        // private readonly _catalogueRepo: CatalogueRepo,
        protected readonly _activedRouter: ActivatedRoute,
        protected readonly _toastService: ToastrService,
        protected readonly _actionStoreSubject: ActionsSubject) {
        super();
    }

    ngOnInit(): void {
        this.initSubmitClickSubscription((action: string) => { this.saveReceipt(action) });

        this._store.dispatch(IsCombineReceipt({ isCombineReceipt: true }));
        this._store.dispatch(RegistTypeReceipt({ data: 'AGENT' }));
        this.subscription = combineLatest([
            this._activedRouter.queryParams,
            this._activedRouter.data
        ]).pipe(
            map(([params, data]) => ({ ...params, ...data })),
            take(1),
            takeUntil(this.ngUnsubscribe)
        ).subscribe(
            (data: any) => {
                if (!!data) {
                    if (data.type === 'existing') {
                        this.conbineType = data.type;
                        this._accountingRepo.getByReceiptCombine(data.arcbno)
                            .pipe(
                                takeUntil(this.ngUnsubscribe)
                            )
                            .subscribe(
                                (res: any) => {
                                    if (!!res) {
                                         this.updateDetailForm(res[0]);
                                    }
                                });
                    }
                }
            }
        );
        this.onAddDatatoList();
    }

    updateDetailForm(data: ReceiptModel) {
        this.CreateReceiptCombineComponent.isUpdate = true;
        const formMapping = {
          paymentDate: !!data.paymentDate ? { startDate: new Date(data.paymentDate), endDate: new Date(data.paymentDate) } : null,
          partnerId: data.customerId,
          contractId: data.agreementId,
          paymentRefNo: data.paymentRefNo,
          exchangeRate: data.exchangeRate,
          currency: data.currencyId,
          combineNo: data.arcbno
        };
    
        this.CreateReceiptCombineComponent.form.patchValue(this.utility.mergeObject({ ...data }, formMapping));
        this.CreateReceiptCombineComponent.form.get('paymentDate').disable();
        this.CreditPaymentReceiptCDCombineComponent.arcbno = data.arcbno;
        this.DebitPaymentReceiptCDCombineComponent.arcbno = data.arcbno;
        this._store.dispatch(UpdateExchangeRateReceiptCombine({ exchangeRate: data.exchangeRate }));
        // this.CreateReceiptCombineComponent.partnerName = data[0].customerName;
    
        this.CreateReceiptCombineComponent.partnerId.setValue(data.customerId);
        this.CreateReceiptCombineComponent.partnerName = data.customerName + ' - ' + data.salemanName;
        this._store.dispatch(SelectPartnerReceiptCombine({
          id: data.customerId,
          shortName: data.customerName,
          accountNo: '',
          partnerNameEn: data.customerName,
          salemanId: data.salemanId,
          salemanName: data.salemanName,
          contractId: data.agreementId
        }))
    }

    updateGeneralReceipt(data: ReceiptModel[]) {
        const dataGeneral = data.filter(x => AccountingConstants.GENERAL_RECEIPT_PAYMENT_METHOD.some(item => item.id === x.paymentMethod));
        const generalReceipts: any[] = [];
        for(const item of dataGeneral) {
          if(item.payments.length === 0){
            continue;
          }
          for(const payment of item.payments){
            const dataPush: any = Object.assign({}, item);
            dataPush.partnerId = payment.partnerId;
            dataPush.partnerName = payment.partnerName;
            dataPush.receiptNo = item.paymentRefNo;
            dataPush.userCreated = item.userNameCreated;
            dataPush.isModified = true;
            dataPush.amountUsd = payment.paidAmountUsd;
            dataPush.amountVnd = payment.paidAmountVnd;
            dataPush.datetimeModified = item.datetimeModified;
            dataPush.notes = payment.notes;
            dataPush.payments = payment;
            generalReceipts.push(dataPush);
          }
        }
        this.ReceiptGeneralCombineComponent.generalReceipts = generalReceipts;
        this.ReceiptGeneralCombineComponent.isContainDraft = generalReceipts.some(x => x.status.toLowerCase() === 'draft');
    }
    
    updateCreditDebitCombineReceipt(data: ReceiptModel[]) {
        const creditList = data.filter(x => AccountingConstants.CREDIT_COMBINE_RECEIPT_PAYMENT_METHOD.some(item => item.id === x.paymentMethod))
            .map((item: any) => item as ReceiptInvoiceModel);
        const debitList = data.filter(x => AccountingConstants.DEBIT_COMBINE_RECEIPT_PAYMENT_METHOD.some(item => item.id === x.paymentMethod))
            .map((item: any) => item as ReceiptInvoiceModel);
        const creditCombineReceipts: IReceiptCombineGroup[] = [];
        if (creditList.length > 0) {
            this._store.dispatch(AddCreditCombineToReceipt({ creditCombineList: creditList }));
        }
        if (debitList.length > 0) {
            this._store.dispatch(AddDebitCombineToReceipt({ debitCombineList: debitList }));
        }
    }

    onAddDatatoList() {
        // Change payment date
        this._actionStoreSubject
            .pipe(
                filter(x => x.type === ReceiptCombineActionTypes.UPDATE_EXCHANGE_RATE),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe((data: any) => {
                if (!!data) {
                    this.generalList$.pipe(take(1))
                        .subscribe(
                            (generalList: any[]) => {
                                if (!!generalList.length) {
                                    this.ReceiptGeneralCombineComponent.generalReceipts.forEach((item: any) => {
                                        if (!!item.amountUsd) {
                                            item.amountVnd = (item.amountUsd * data.exchangeRate).toFixed(0) || 0;
                                        }
                                    });
                                }
                            });
                }
            });

        // Update general receipt
        this._actionStoreSubject
        .pipe(
            filter(x => x.type === ReceiptCombineActionTypes.ADD_GENERAL_COMBINE_TO_RECEIPT),
            takeUntil(this.ngUnsubscribe)
        )
        .subscribe((data: any) => {
            if(!data.generalCombineList.length) {
                this.CreateReceiptCombineComponent.isSubmitted = true;
            }else{
                // 
                this.generalList$.pipe(takeUntil(this.ngUnsubscribe))
                .subscribe(
                    (generalList: any[]) => {
                        if(!generalList.length) {
                            this.ReceiptGeneralCombineComponent.generalReceipts = [];
                        }
                        if (!!generalList.length) {
                            this.ReceiptGeneralCombineComponent.generalReceipts = generalList;
                        }else{
                            this.ReceiptGeneralCombineComponent.generalReceipts = [];
                        }
                    });
            }
        });

        // Update credit receipt
        this._actionStoreSubject
        .pipe(
            filter(x => x.type === ReceiptCombineActionTypes.ADD_CREDIT_COMBINE_TO_RECEIPT),
            takeUntil(this.ngUnsubscribe)
        )
        .subscribe(
            (data: any) => {
                if (!!data) {
                    this.creditList$.pipe(takeUntil(this.ngUnsubscribe))
                    .subscribe(
                        (creditList: ReceiptInvoiceModel[]) => {
                            if (!!creditList.length) {
                                const creditGroup = groupBy(creditList, 'officeId')
                                let results = [];
                                Object.entries(creditGroup).forEach((value, index) => {
                                    const currentDebit = creditList.find(x => x.officeId === value[0]);
                                    // const _total = value[1].reduce((acc, curr) => (acc += curr.totalAmount), 0);
                                    let itemGrps: any = {
                                        id: SystemConstants.EMPTY_GUID,
                                        officeId: value[0],
                                        officeName: currentDebit.officeName,
                                        paymentMethod: this.CreditPaymentReceiptCDCombineComponent.paymentMethodsCredit[0].value,
                                        receiptNo: '',
                                        description: '',
                                        cdCombineList: value[1],
                                        sumTotal: {}
                                    }
                                    itemGrps = this.CreditPaymentReceiptCDCombineComponent.calculateTotal(itemGrps);
                                    results.push(itemGrps);
                                });
                                this.receiptCreditGroups = results;
                                // this._store.dispatch(RegistCreditCombineGroupSuccess({  creditCombineGroup: this.receiptCreditGroups}));
                                this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups = results;
                                // this.CreditPaymentReceiptCDCombineComponent.calculateTotal(results);
                            }else{
                                this.receiptCreditGroups = [];
                                this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups = [];
                            }
                        }
                    )
                }
            }
        )

        // Update debit receipt
        this._actionStoreSubject
            .pipe(
                filter(x => x.type === ReceiptCombineActionTypes.ADD_DEBIT_COMBINE_TO_RECEIPT),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.debitList$.pipe(takeUntil(this.ngUnsubscribe))
                        .subscribe(
                            (debitList: ReceiptInvoiceModel[]) => {
                                if (!!debitList.length) {
                                    const debitGroup = groupBy(debitList, 'officeId')
                                    let results = [];
                                    Object.entries(debitGroup).forEach((value, index) => {
                                        const currentDebit = debitList.find(x => x.officeId === value[0]);
                                        // const _total = value[1].reduce((acc, curr) => (acc += curr.totalAmount), 0);
                                        let itemGrps: any = {
                                            id: SystemConstants.EMPTY_GUID,
                                            officeId: value[0],
                                            officeName: currentDebit.officeName,
                                            paymentMethod: this.CreditPaymentReceiptCDCombineComponent.paymentMethodsDebit[0].value,
                                            receiptNo: '',
                                            description: '',
                                            cdCombineList: value[1],
                                            sumTotal: {}
                                        }
                                        itemGrps = this.CreditPaymentReceiptCDCombineComponent.calculateTotal(itemGrps);
                                        results.push(itemGrps);
                                    });
                                    this.receiptDebitGroups = results;
                                    this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups = results;
                                    // this.CreditPaymentReceiptCDCombineComponent.receiptDebitGroups = results;
                                }else{
                                    this.receiptDebitGroups = [];
                                    this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups = [];
                                }
                            }
                        )
                    }
                }
            )
    }

    confirmDoneReceipt() {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            body: 'Noted: After you save the receipt, you can not edit. Are you sure do this action?',
            title: 'Alert',
            labelCancel: 'No',
            labelConfirm: 'Yes',
            iconConfirm: 'la la-save'
        }, () => {
            this.submitClick('done');
        })
    }
    
    saveReceipt(actionString: string, type: string = null, receipt: any = null) {
        let action: number;
        if (this.conbineType === 'existing' && actionString === 'draft') {
            actionString = 'update';
        }
        switch (actionString) {
            case 'draft':
                action = SaveReceiptActionEnum.DRAFT_CREATE;
                break;
            case 'update':
                action = SaveReceiptActionEnum.DRAFT_UPDATE;
                break;
            case 'done':
                action = SaveReceiptActionEnum.DONE;
                break;
            case 'cancel':
                action = SaveReceiptActionEnum.CANCEL;
                break;
            default:
                break;
        }
        this.getFormData();
        let resultModel = [];
        if (!this.checkValidGeneralReceipts(type)) {
            return;
        }

        if (!type || type === 'general') {
            const generalReceipts = this.getGeneralReceipts(actionString, receipt?.id);
            if (generalReceipts?.length) {
                resultModel = [...generalReceipts];
            }
        }

        if (!type || type === 'credit') {
            const creditReceipts = this.getDataCredit(actionString, receipt?.id);
            if (creditReceipts?.length) {
                resultModel = [...resultModel, ...creditReceipts];
            }
        }

        if (!type || type === 'debit') {
            const debitReceipts = this.getDataDebit(actionString, receipt?.id);
            if (debitReceipts?.length) {
                resultModel = [...resultModel, ...debitReceipts];
            }
        }

        if (!resultModel.length) {
            this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
            return;
        }
        console.log('model save', resultModel);
        // return;
        this.onSaveDataReceipt(resultModel, action);
    }

    onSaveDataReceipt(models: any, action: number) {
        this._accountingRepo.saveCombineReceipt(models, action)
            .pipe(concatMap((res: CommonInterface.IResult) => {
                if (res.status) {
                    this._toastService.success(res.message);
                    this._store.dispatch(ResetCombineInvoiceList());
                    this._store.dispatch(ResetInvoiceList());
                    if(!this.formCreateMapValue.arcbno || this.conbineType === 'existing'){
                        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}/receipt/combine/${res.data[0].arcbno}`]);
                        return EMPTY;
                    }
                    return this._accountingRepo.getByReceiptCombine(res.data[0].arcbno);
                }else{
                    this._toastService.warning(res.message);
                }
                of(res);
            }),
            takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: any) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                        return;
                    }
                    this.updateDetailForm(res[0]);
                    this.CreateReceiptCombineComponent.isContainDraft = res.some(x => x.status === 'Done' && x?.syncStatus !== 'Synced');
                    this.updateGeneralReceipt(res);
                    this.updateCreditDebitCombineReceipt(res);
                },
                // (res: HttpErrorResponse) => {
                //     this._toastService.error(res?.message);
                // }
            )
    };

    getDataCredit(action: string, id: string = null) {
        let receiptModels: ReceiptModel[] = [];
        this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups
        .filter((x: any) => (!x.status || x.status.toLowerCase() === 'draft') && (!id || x.id === id))
        .forEach((item: IReceiptCombineGroup) => {
            let model: ReceiptModel = new ReceiptModel(item);
            model.id = action !== 'draft' ? item.id : SystemConstants.EMPTY_GUID;
            model.type = 'CREDIT';
            model.paymentRefNo = item.receiptNo;
            model.customerId = this.formCreateMapValue.customerId;
            model.currencyId = this.formCreateMapValue.currencyId;
            model.paymentMethod = model.class = item.paymentMethod;
            model.paymentDate = this.formCreateMapValue.paymentDate;
            model.arcbno = this.CreateReceiptCombineComponent.combineNo.value;
            model.creditAmountUsd = model.finalPaidAmountUsd = item.cdCombineList.reduce((acc, curr) => (acc += curr.paidAmountUsd), 0);
            model.creditAmountVnd = model.finalPaidAmountVnd = item.cdCombineList.reduce((acc, curr) => (acc += curr.paidAmountVnd), 0);
            model.description = item.description;
            model.agreementId = this.formCreateMapValue.contractId;
            model.exchangeRate = this.formCreateMapValue.exchangeRate;
            item.cdCombineList.forEach((x: any) => {
                const payment = x;
                payment.type = 'CREDIT';
                payment.paymentType = 'CREDIT';
                payment.amount = payment.unpaidAmountUsd;
                payment.totalPaidUsd = payment.paidAmountUsd;
                payment.totalPaidVnd = payment.paidAmountVnd;
                payment.remainAmount = payment.remainAmountUsd;
                model.payments.push(payment);
            })
            receiptModels.push(model);
        })
        return receiptModels;
    }

    getDataDebit(action: string, id: string = null) {
        let receiptModels: ReceiptModel[] = [];
        this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups
        .filter((x: any) => (!x.status || x.status.toLowerCase() === 'draft') && (!id || x.id === id))
        .forEach((item: IReceiptCombineGroup) => {
            let model: ReceiptModel = new ReceiptModel(item);
            model.id = action !== 'draft' ? item.id : SystemConstants.EMPTY_GUID;
            model.type = 'DEBIT';
            model.paymentRefNo = item.receiptNo;
            model.customerId = this.formCreateMapValue.customerId;
            model.currencyId = this.formCreateMapValue.currencyId;
            model.paymentMethod = model.class = item.paymentMethod;
            model.paymentDate = this.formCreateMapValue.paymentDate;
            model.creditAmountUsd = model.finalPaidAmountUsd = item.cdCombineList.reduce((acc, curr) => (acc += curr.paidAmountUsd), 0);
            model.creditAmountVnd = model.finalPaidAmountVnd = item.cdCombineList.reduce((acc, curr) => (acc += curr.paidAmountVnd), 0);
            model.description = item.description;
            model.arcbno = this.CreateReceiptCombineComponent.combineNo.value;
            model.agreementId = this.formCreateMapValue.contractId;
            model.exchangeRate = this.formCreateMapValue.exchangeRate;
            item.cdCombineList.forEach((x: any) => {
                const payment = x;
                payment.type = 'DEBIT';
                payment.paymentType = 'DEBIT';
                payment.amount = payment.unpaidAmountUsd;
                payment.totalPaidUsd = payment.paidAmountUsd;
                payment.totalPaidVnd = payment.paidAmountVnd;
                payment.remainAmount = payment.remainAmountUsd;
                model.payments.push(payment);
            })
            receiptModels.push(model);
        })
        return receiptModels;
    }

    getFormData(){
        const dataForm: any = this.CreateReceiptCombineComponent.form.getRawValue();
        this._store.select(ReceiptCombineAgreementState)
        .subscribe((res: any) => {
            this.formCreateMapValue = {
                arcbno: this.CreateReceiptCombineComponent.combineNo.value,
                customerId: dataForm.partnerId,
                paymentDate: !!dataForm.paymentDate.startDate ? formatDate(dataForm.paymentDate.startDate, 'yyyy-MM-dd', 'en') : null,
                exchangeRate: !!dataForm.exchangeRate ? dataForm.exchangeRate : 1,
                currencyId: !!dataForm.currency ? dataForm.currency : null,
                contractId: res
            };
        });
        
    }

    getGeneralReceipts(action: string, id: string = null) {
        let receiptModels: ReceiptModel[] = [];

        if (!!id) {
            const generals = this.ReceiptGeneralCombineComponent.generalReceipts
                .filter((x: any) => (!x.status || x.status.toLowerCase() === 'draft') && (!id || x.id === id));
            let item: ReceiptModel = new ReceiptModel(generals[0]);
            item.type = 'Agent';
            item.currencyId = this.formCreateMapValue.currencyId;
            item.customerId = this.formCreateMapValue.customerId;
            item.paymentDate = !this.formCreateMapValue.paymentDate ? null : this.formCreateMapValue.paymentDate;
            item.agreementId = this.formCreateMapValue.contractId;
            item.arcbno = this.CreateReceiptCombineComponent.combineNo.value;
            item.exchangeRate = this.formCreateMapValue.exchangeRate;
            item.payments = [];
            generals.forEach((payment: any) => {
                payment.payments.partnerId = payment.partnerId;
                payment.payments.type = payment.paymentMethod;
                payment.payments.paymentType = 'OTHER';
                payment.payments.paidAmountUsd = payment.amountUsd;
                payment.payments.paidAmountVnd = payment.amountVnd;
                payment.payments.obhpartnerId = payment.obhpartnerId;
                payment.payments.officeId = payment.officeId;
                payment.payments.notes = payment.notes;

                item.payments.push(payment.payments);
            })
            receiptModels.push(item);
        } else {
            this.ReceiptGeneralCombineComponent.generalReceipts
                .filter((x: any) => (!x.status || x.status.toLowerCase() === 'draft') && (!id || x.id === id))
                .forEach((element: any) => {
                    let item: ReceiptModel = new ReceiptModel(element);
                    item.id = element.id;
                    item.type = 'Agent';
                    item.officeId = element.officeId;
                    item.currencyId = this.formCreateMapValue.currencyId;
                    item.customerId = this.formCreateMapValue.customerId;
                    item.paymentMethod = element.paymentMethod;
                    item.paymentDate = !this.formCreateMapValue.paymentDate ? null : this.formCreateMapValue.paymentDate;
                    item.paidAmount = item.paidAmountUsd = item.finalPaidAmount = item.finalPaidAmountUsd = element.amountUsd;//this.ReceiptGeneralCombineComponent.generalReceipts.reduce((acc, curr) => (acc += curr.amountUsd), 0);
                    item.paidAmountVnd = item.finalPaidAmountVnd = element.amountVnd;//this.ReceiptGeneralCombineComponent.generalReceipts.reduce((acc, curr) => (acc += curr.amountVnd), 0);
                    item.obhpartnerId = element.obhPartnerId;
                    // item.description = element.notes;
                    item.agreementId = this.formCreateMapValue.contractId;//element.agreementId;
                    item.arcbno = this.CreateReceiptCombineComponent.combineNo.value;
                    item.exchangeRate = this.formCreateMapValue.exchangeRate;

                    if (!element?.payments) {
                        const payment: any = {
                            id: SystemConstants.EMPTY_GUID,
                            partnerId: element.partnerId,
                            type: element.paymentMethod,
                            paymentType: 'OTHER',
                            paidAmountUsd: element.amountUsd,
                            paidAmountVnd: element.amountVnd,
                            officeId: element.officeId,
                            notes: element.notes
                        };
                        item.payments.push(payment);
                    } else {
                        item.payments = [];
                        element.payments.partnerId = element.partnerId;
                        element.payments.type = element.paymentMethod;
                        element.payments.paymentType = 'OTHER';
                        element.payments.paidAmountUsd = element.amountUsd;
                        element.payments.paidAmountVnd = element.amountVnd;
                        element.payments.notes = element.notes;
                        element.payments.officeId = element.officeId;
                        item.payments.push(element.payments);
                    }
                    receiptModels.push(item);
                });
        }
        return receiptModels;
    }


    checkValidGeneralReceipts(type: string = null) {
        if (!type || type === 'general') {
            this.ReceiptGeneralCombineComponent.isSubmitted = true;
            for (const item of this.ReceiptGeneralCombineComponent.generalReceipts) {
                if (!item.partnerId
                    || !item.paymentMethod
                    || item.amountUsd === null
                    || item.amountUsd < 0
                    || item.amountVnd === null
                    || item.amountVnd < 0
                    || !item.officeId) {
                    return false;
                }
            }

            this.ReceiptGeneralCombineComponent.generalReceipts.forEach((obj) => {
                if (this.ReceiptGeneralCombineComponent.generalReceipts.filter(item => item.partnerId === obj.partnerId && item.paymentMethod === obj.paymentMethod && item.obhPartnerId === obj.obhPartnerId && item.officeId === obj.officeId).length > 1) {
                    obj.duplicate = true;
                }else{
                    obj.duplicate = false;
                }
            });
            if(this.ReceiptGeneralCombineComponent.generalReceipts.some(item => item.duplicate))
                return false;
        }

        if (!type || type === 'credit') {
            this.CreditPaymentReceiptCDCombineComponent.isSubmitted = true;
            for (const cdList of this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups) {
                if(!cdList.cdCombineList.length){
                    this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
                    return false;
                }
                if (cdList.cdCombineList.some(item => item.paidAmountUsd < 0 || item.paidAmountUsd === null || item.paidAmountUsd > item.unpaidAmountUsd))
                    return false;
            }
        }
        if (!type || type === 'debit') {
            this.DebitPaymentReceiptCDCombineComponent.isSubmitted = true;
            for (const cdList of this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups) {
                if(!cdList.cdCombineList.length){
                    this._toastService.warning("Receipt don't have any invoice in this period, Please check it again!");
                    return false;
                }
                if (cdList.cdCombineList.some(item => item.paidAmountUsd < 0 || item.paidAmountUsd === null || item.paidAmountUsd > item.unpaidAmountUsd))
                    return false;
            }
        }
        return true;
    }

    onSaveReceipt(data: any) {
        this.saveReceipt(data.action, data.type, data.receipt);
    }
    
      
}
