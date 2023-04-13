import { Component, EventEmitter, OnInit, Output, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AccountingConstants, RoutingConstants, SystemConstants } from '@constants';
import { IReceiptCombineGroup, ReceiptInvoiceModel, ReceiptModel } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { AccountingRepo, CatalogueRepo } from '@repositories';
import { IAppState } from '@store';
import { ToastrService } from 'ngx-toastr';
import { pluck, switchMap, takeUntil, filter } from 'rxjs/operators';
import { ARCustomerPaymentReceiptCDCombineComponent } from '../components/cd-combine/receipt-cd-combine.component';
import { ARCustomerPaymentCreateReciptCombineComponent } from '../create-receipt-combine/create-receipt-combine.component';
import _groupBy from 'lodash/groupBy';
import { AddCreditCombineToReceipt, AddDebitCombineToReceipt, IsCombineReceipt, ReceiptCombineActionTypes, RegistTypeReceipt, ResetCombineInvoiceList, ResetInvoiceList } from '../store/actions';

@Component({
  selector: 'app-detail-receipt-combine',
  templateUrl: './detail-receipt-combine.component.html',
  styleUrls: ['./detail-receipt-combine.component.scss']
})
export class DetailReceiptCombineComponent extends ARCustomerPaymentCreateReciptCombineComponent implements OnInit {
  @ViewChild('CreditPayment') CreditPaymentReceiptCDCombineComponent: ARCustomerPaymentReceiptCDCombineComponent;
  @ViewChild('DebitPayment') DebitPaymentReceiptCDCombineComponent: ARCustomerPaymentReceiptCDCombineComponent;

  @Output() onAddCreditCombine: EventEmitter<any> = new EventEmitter<any>();
  
  constructor(protected _store: Store<IAppState>,
    protected _accountingRepo: AccountingRepo,
    protected _router: Router,
    protected _activedRouter: ActivatedRoute,
    protected _toastService: ToastrService,
    protected _actionStoreSubject: ActionsSubject) {
    super(_store, _accountingRepo, _router, _activedRouter, _toastService, _actionStoreSubject);
}

  ngOnInit() {
    this.subscriptRouterChangeToGetDetailReceipt();
    this._store.dispatch(IsCombineReceipt({ isCombineReceipt: true }));
    this._store.dispatch(RegistTypeReceipt({ data: 'AGENT' }));
    this.initSubmitClickSubscription((action: string) => { this.saveReceipt(action) });
    this.subscriptActionValueChange();
  }

  subscriptRouterChangeToGetDetailReceipt() {
    this._activedRouter.params
      .pipe(
        filter(x => !!x),
        pluck('arcbno'),
        switchMap((arcbno: string) => this._accountingRepo.getByReceiptCombine(arcbno)),
        takeUntil(this.ngUnsubscribe),
      )
      .subscribe(
        (res: any) => {
          if (!!res) {
            if (res.id === SystemConstants.EMPTY_GUID) {
              return;
            }
            this.updateDetailForm(res[0]);
            this.CreateReceiptCombineComponent.isContainDraft = res.some(x => x.status === 'Done' && x?.syncStatus !== 'Synced');
            this.updateGeneralReceipt(res);
            this.updateCreditDebitCombineReceipt(res);
          }
        },
      );
  }

  getDetailReceiptCombine(arcbno: string) {
    this._accountingRepo.getByReceiptCombine(arcbno)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(
        (res: any) => {
          if (!!res) {
            if (res.id === SystemConstants.EMPTY_GUID) {
              this.gotoList();
              return;
            }
            this.updateDetailForm(res[0]);
            this.CreateReceiptCombineComponent.isContainDraft = res.some(x => x.status === 'Done' && x?.syncStatus !== 'Synced');
            this.updateGeneralReceipt(res);
            this.updateCreditDebitCombineReceipt(res);
          } else {
            this.gotoList();
          }
        },
        (err) => this.gotoList()
      );
  }

  subscriptActionValueChange() {
    this._actionStoreSubject
    .pipe(
      filter(x => x.type === ReceiptCombineActionTypes.ADD_GENERAL_COMBINE_TO_RECEIPT),
      takeUntil(this.ngUnsubscribe)
    )
    .subscribe((data: any) => {
      if (!data.length) {
        this.CreateReceiptCombineComponent.isSubmitted = true;
      }
    });
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
                  const creditGroup = creditList.reduce((acc, curr) => {
                    const key = `${curr.officeId}_${curr.paymentRefNo}`; // * guid_guiid
                    if (!acc[key]) {
                      acc[key] = [];
                    }
                    acc[key].push(curr);
                    return acc;
                  }, {} as { [key: string]: { officeId: string, paymentRefNo: string }[] });

                  const creditResult = Object.entries(creditGroup).map(([key]) => {
                    const [officeId, paymentRefNo] = key.split("_");
                    return {
                      officeId,
                      paymentRefNo,
                      items: creditGroup[key]
                    };
                  });

                  let results = [];
                  creditResult.forEach((value, index) => {
                    const currentDebit: any = value.items[0];
                    let itemGrps: any = {
                      id: !currentDebit.id ? SystemConstants.EMPTY_GUID : currentDebit.id,
                      officeId: value.officeId,
                      officeName: currentDebit.officeName,
                      partnerId: currentDebit.customerId,
                      paymentMethod: this.CreditPaymentReceiptCDCombineComponent.paymentMethodsCredit[0].value,
                      receiptNo: value.paymentRefNo,
                      description: currentDebit.description,
                      status: currentDebit.status,
                      syncStatus: currentDebit.syncStatus,
                      cdCombineList: [],
                      sumTotal: {}
                    }
                    value.items.forEach((element: any) => {
                      if (!!element.payments && !!element.payments.length) {
                        element.payments.forEach(item => {
                          const data = item;
                          data.remainAmount = item.unpaidAmountUsd - item.paidAmountUsd;
                          data.remainAmountUsd = item.unpaidAmountUsd - item.paidAmountUsd;
                          data.remainAmountVnd = item.unpaidAmountVnd - item.paidAmountVnd;
                          itemGrps.cdCombineList.push(data);
                        });
                      } else {
                        itemGrps.cdCombineList.push(element);
                      }
                      itemGrps.officeName = element.officeName;

                    });

                    itemGrps = this.CreditPaymentReceiptCDCombineComponent.calculateTotal(itemGrps);
                    results.push(itemGrps);
                  });
                  this.receiptCreditGroups = results;
                  this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups = results;
                  this.CreditPaymentReceiptCDCombineComponent.isContainDraftCredit = results.some(x => !x.status || x.status.toLowerCase() === 'draft');
                }
              }
            )
        }
      }
    )

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
                  const debitGroup = debitList.reduce((acc, curr) => {
                    const key = `${curr.officeId}_${curr.paymentRefNo}`; // * guid_guiid
                    if (!acc[key]) {
                      acc[key] = [];
                    }
                    acc[key].push(curr);
                    return acc;
                  }, {} as { [key: string]: { officeId: string, paymentRefNo: string }[] });

                  const debitResult = Object.entries(debitGroup).map(([key]) => {
                    const [officeId, paymentRefNo] = key.split("_");
                    return {
                      officeId,
                      paymentRefNo,
                      items: debitGroup[key]
                    };
                  });

                  let results = [];
                  debitResult.forEach((value, index) => {
                    const currentDebit : any = value.items[0];
                    let itemGrps: any = {
                      id: !currentDebit.id ? SystemConstants.EMPTY_GUID : currentDebit.id,
                      officeId: value.officeId,
                      officeName: currentDebit.officeName,
                      partnerId: currentDebit.customerId,
                      paymentMethod: this.CreditPaymentReceiptCDCombineComponent.paymentMethodsDebit[0].value,
                      receiptNo: value.paymentRefNo,
                      description: currentDebit.description,
                      status: currentDebit.status,
                      syncStatus: currentDebit.syncStatus,
                      cdCombineList: [],
                      sumTotal: {}
                    }
                    value.items.forEach((element: any) => {
                      if (!!element.payments && !!element.payments.length) {
                        element.payments.forEach(item => {
                          const data = item;
                          data.remainAmount = item.unpaidAmountUsd - item.paidAmountUsd;
                          data.remainAmountUsd = item.unpaidAmountUsd - item.paidAmountUsd;
                          data.remainAmountVnd = item.unpaidAmountVnd - item.paidAmountVnd;
                          itemGrps.cdCombineList.push(data);
                        });
                      } else {
                        itemGrps.cdCombineList.push(element);
                      }
                        itemGrps.officeName = element.officeName;
                      // });

                    });
                    itemGrps = this.CreditPaymentReceiptCDCombineComponent.calculateTotal(itemGrps);
                    results.push(itemGrps);
                  });

                  this.receiptDebitGroups = results;
                  this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups = results;
                  this.DebitPaymentReceiptCDCombineComponent.isContainDraftDebit = results.some(x => !x.status || x.status.toLowerCase() === 'draft');
                }
              }
            )
        }
      }
    )
  }
  
  
  onSynceCombineToAccountant(isSynce: any) {
    this.getFormData();
    const receiptSyncIds: AccountingInterface.IRequestString[] = [];
    const generalGroup = (_groupBy(this.ReceiptGeneralCombineComponent.generalReceipts, 'id') || []);
    const generalReceipts = Object.keys(generalGroup).map(key => generalGroup[key][0]);
    generalReceipts
      .filter((x: any) => !!x.status && x.status.toLowerCase() === 'done')
      .forEach((element: any) => {
        const receiptSyncId: AccountingInterface.IRequestString = {
          id: element.id,
          action: element.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
        };
        receiptSyncIds.push(receiptSyncId);
      });

    this.DebitPaymentReceiptCDCombineComponent.receiptDebitGroups
      .filter((x: any) => !!x.status && x.status.toLowerCase() === 'done')
      .forEach((item: IReceiptCombineGroup) => {
        const receiptSyncId: AccountingInterface.IRequestString = {
          id: item.id,
          action: item.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
        };
        receiptSyncIds.push(receiptSyncId);
      });

    this.CreditPaymentReceiptCDCombineComponent.receiptCreditGroups
      .filter((x: any) => !!x.status && x.status.toLowerCase() === 'done')
      .forEach((item: IReceiptCombineGroup) => {
        const receiptSyncId: AccountingInterface.IRequestString = {
          id: item.id,
          action: item.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
        };
        receiptSyncIds.push(receiptSyncId);
      });
    if (!receiptSyncIds.length) {
      this._toastService.warning("Please done receipt before sync data to accountant, Please check it again!");
      return;
    }

    this._accountingRepo.syncReceiptToAccountant(receiptSyncIds)
      .pipe(
    ).subscribe(
      (res: CommonInterface.IResult) => {
        if (((res as CommonInterface.IResult).status)) {
          this._toastService.success("Send Data to Accountant System Successful");
          this._store.dispatch(ResetInvoiceList());
          this._store.dispatch(ResetCombineInvoiceList());
          this.getDetailReceiptCombine(this.CreateReceiptCombineComponent.combineNo.value);
        } else {
          this._toastService.error("Send Data Fail");
        }
      },
      (error) => {
        console.log(error);
      }
    );
  }

  onSaveGeneralReceipt(data: any) {
    if (data.action === 'delete'){
      this.onConfirmDeleteCP(data.receipt);
    }else{
      this.onSaveReceipt(data);
    }
  }

  onConfirmDeleteCP(selectedCPs: any) {
    this._accountingRepo
      .deleteCusPayment(selectedCPs.id)
      .subscribe((res: any) => {
        this._toastService.success(res.message);
        this._store.dispatch(ResetCombineInvoiceList());
        this._store.dispatch(ResetInvoiceList());
        this.getDetailReceiptCombine(selectedCPs.arcbno);
      });
  }

  gotoList() {
    this._store.dispatch(ResetInvoiceList());
    this._store.dispatch(ResetCombineInvoiceList());
    this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);
  }
}
