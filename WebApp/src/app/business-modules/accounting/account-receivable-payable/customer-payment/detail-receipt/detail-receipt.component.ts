import { Component, OnInit, ViewChild } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { AccountingRepo } from '@repositories';
import { ReceiptModel } from '@models';
import { SystemConstants, AccountingConstants, RoutingConstants } from '@constants';

import { ARCustomerPaymentCreateReciptComponent } from '../create-receipt/create-receipt.component';

import { of } from 'rxjs';
import { pluck, switchMap, tap, concatMap, takeUntil } from 'rxjs/operators';
import { IAppState } from '@store';
import { Store } from '@ngrx/store';
import { GetInvoiceListSuccess, ResetInvoiceList, RegistTypeReceipt } from '../store/actions';
import { ARCustomerPaymentFormCreateReceiptComponent } from '../components/form-create-receipt/form-create-receipt.component';
import { InjectViewContainerRefDirective } from '@directives';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'app-detail-receipt',
    templateUrl: './detail-receipt.component.html',
})
export class ARCustomerPaymentDetailReceiptComponent extends ARCustomerPaymentCreateReciptComponent implements OnInit {
    @ViewChild(ARCustomerPaymentFormCreateReceiptComponent) formCreate: ARCustomerPaymentFormCreateReceiptComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;

    receiptId: string;
    receiptDetail: ReceiptModel;
    confirmMessage: string = '';

    constructor(
        protected _router: Router,
        protected _toast: ToastrService,
        protected _accountingRepo: AccountingRepo,
        protected _activedRoute: ActivatedRoute,
        protected _store: Store<IAppState>,
    ) {
        super(_router, _toast, _accountingRepo, _activedRoute, _store);
    }

    ngOnInit() {
        this.subscriptRouterChangeToGetDetailReceipt();

        this.initSubmitClickSubscription((actionType: string) => this.saveReceipt(actionType));
    }

    subscriptRouterChangeToGetDetailReceipt() {
        this._activedRoute.params
            .pipe(
                pluck('id'),
                tap((id: string) => { this.receiptId = id }),
                switchMap((receiptId: string) => this._accountingRepo.getDetailReceipt(receiptId)),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe(
                (res: ReceiptModel) => {
                    if (!!res) {
                        if (res.id === SystemConstants.EMPTY_GUID) {
                            this.gotoList();
                            return;
                        }
                        this.updateDetailForm(res);
                    } else this.gotoList();
                },
                (err) => this.gotoList()
            );
    }

    getDetailReceipt(id: string) {
        this._accountingRepo.getDetailReceipt(id)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe(
                (res: ReceiptModel) => {
                    if (!!res) {
                        if (res.id === SystemConstants.EMPTY_GUID) {
                            this.gotoList();
                            return;
                        }
                        this.updateDetailForm(res);
                    } else this.gotoList();
                },
                (err) => this.gotoList()
            )
    }

    updateDetailForm(res: ReceiptModel) {
        this.receiptDetail = res;
        this._store.dispatch(RegistTypeReceipt({ data: res.type }));

        this.updateFormCreate(this.receiptDetail);
        this.updateListInvoice(this.receiptDetail);
    }

    updateFormCreate(res: ReceiptModel) {
        const formMapping = {
            date: !!res.fromDate && !!res.toDate ? { startDate: new Date(res.fromDate), endDate: new Date(res.toDate) } : null,
            customerId: res.customerId,
            agreementId: res.agreementId,
            paymentRefNo: res.paymentRefNo
        };

        this.formCreate.formSearchInvoice.patchValue(formMapping);
        this.formCreate.customerName = res.customerName;
        this.formCreate.getContract();
    }

    updateListInvoice(res: ReceiptModel) {
        const formMapping = {
            type: res.type?.split(","),
            paymentDate: !!res.paymentDate ? { startDate: new Date(res.paymentDate), endDate: new Date(res.paymentDate) } : null,
        };

        this.listInvoice.form.patchValue(this.utility.mergeObject({ ...res }, formMapping));

        this._store.dispatch(GetInvoiceListSuccess({ invoices: res.payments }));
        (this.listInvoice.customerInfo as any) = { id: res.customerId };

        if (res.status === AccountingConstants.RECEIPT_STATUS.DONE || res.status === AccountingConstants.RECEIPT_STATUS.CANCEL) {
            this.listInvoice.isReadonly = true;
            this.formCreate.isReadonly = true;
        }
    }


    onSaveDataReceipt(model: ReceiptModel, action: number) {
        model.id = this.receiptDetail.id;
        model.userCreated = this.receiptDetail.userCreated;
        model.userModified = this.receiptDetail.userModified;
        model.datetimeCreated = this.receiptDetail.datetimeCreated;
        model.datetimeModified = this.receiptDetail.datetimeModified;
        model.status = this.receiptDetail.status;
        model.syncStatus = this.receiptDetail.syncStatus;
        model.lastSyncDate = this.receiptDetail.lastSyncDate;

        if (!action) { return; };
        this._accountingRepo.saveReceipt(model, action)
            .pipe(
                concatMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this._store.dispatch(ResetInvoiceList());
                        return this._accountingRepo.getDetailReceipt(this.receiptId);
                    }
                    of(res);
                }),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (res: any) => {
                    if (!!res && res.status === false) {
                        this._toastService.error(res.message);
                        return;
                    }
                    this.updateDetailForm(res);
                },
                (res: HttpErrorResponse) => {
                    if (res.error.code === SystemConstants.HTTP_CODE.EXISTED) {
                        this.formCreate.paymentRefNo.setErrors({ existed: true });
                    }
                }
            )
    };

    onSyncBravo() { }

    gotoList() {
        this._store.dispatch(ResetInvoiceList());
        this._router.navigate([`${RoutingConstants.ACCOUNTING.ACCOUNT_RECEIVABLE_PAYABLE}`]);

    }

    confirmSync() {
        this.confirmMessage = `Are you sure you want to send data to accountant system?`;
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
            title: 'Sync To Accountant System',
            body: 'Are you sure you want to send data to accountant system',
            iconConfirm: 'la la-cloud-upload',
            labelConfirm: 'Yes'
        }, () => {
            this.sendReceiptToAccountant();
        });
    }

    sendReceiptToAccountant() {
        const receiptSyncIds: AccountingInterface.IRequestString[] = [];
        const receiptSyncId: AccountingInterface.IRequestString = {
            id: this.receiptDetail.id,
            action: this.receiptDetail.syncStatus === AccountingConstants.SYNC_STATUS.REJECTED ? 'UPDATE' : 'ADD',
        };
        receiptSyncIds.push(receiptSyncId);

        this._accountingRepo.syncReceiptToAccountant(receiptSyncIds)
            .pipe(
            ).subscribe(
                (res: CommonInterface.IResult) => {
                    if (((res as CommonInterface.IResult).status)) {
                        this._toastService.success("Send Data to Accountant System Successful");

                        this.getDetailReceipt(this.receiptId);
                    } else {
                        this._toastService.error("Send Data Fail");
                    }
                },
                (error) => {
                    console.log(error);
                }
            );
    }

    confirmCancel() {
        if (this.receiptDetail.status === AccountingConstants.RECEIPT_STATUS.CANCEL) {
            this.gotoList();
        } else {
            this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainerRef.viewContainerRef, {
                body: 'Do you want to exit without saving?',
            }, () => {
                this.gotoList();
            })
        }
    }
}
