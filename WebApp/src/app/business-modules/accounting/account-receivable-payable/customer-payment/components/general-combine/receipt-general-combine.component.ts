import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { ConfirmPopupComponent, Permission403PopupComponent } from '@common';
import { AccountingConstants, JobConstants, SystemConstants } from '@constants';
import { InjectViewContainerRefDirective } from '@directives';
import { ContractPartner, GeneralCombineReceiptModel, Office, Partner } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { AccountingRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { getCurrentUserState } from '@store';
import { cloneDeep } from 'lodash';
import { forkJoin } from 'rxjs';
import { filter, map, skipWhile, switchMap, takeUntil, tap } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { AddGeneralCombineToReceipt, ReceiptCombineActionTypes, RemoveDebitCombine } from '../../store/actions';
import { ICustomerPaymentState, ReceiptCombineExchangeState, ReceiptCombinePartnerState, ReceiptCombineSalemanState } from '../../store/reducers';
import { CommonEnum } from '@enums';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'receipt-general-combine',
    templateUrl: './receipt-general-combine.component.html',
    styleUrls: ['./receipt-general-combine.component.scss']
})
export class ARCustomerPaymentReceiptGeneralCombineComponent extends AppList implements OnInit {
    @ViewChild(InjectViewContainerRefDirective) viewContainer: InjectViewContainerRefDirective;
    
    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    get readonly(): boolean {
        return this._readonly;
    }
    private _readonly: boolean = false;
    @Input() isUpdate: boolean = false;
    @Output() onSaveReceipt: EventEmitter<Partial<any>> = new EventEmitter<Partial<any>>();
    
    generalReceipts: GeneralCombineReceiptModel[] = [];
    partners: ContractPartner[] = [];
    obhPartners: Partner[] = [];
    offices: Office[] = []
    paymentMethods = AccountingConstants.GENERAL_RECEIPT_PAYMENT_METHOD;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    isSubmitted: boolean = false;

    exchangeRate: number;
    partnerId: any;
    isContainDraft: boolean = false;
    currentOffice: string = '';

    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _actionStoreSubject: ActionsSubject,
        private readonly _accountingRepo: AccountingRepo,
        private readonly _toastService: ToastrService
    ) {
        super();
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Agency Name', field: '', width: 200, required: true },
            { title: 'Payment Method', field: '', required: true, width: 150 },
            { title: 'Amount', field: '', required: true },
            { title: 'Amount VND', field: '', required: true },
            { title: 'OBH Branch', field: '' },
            { title: 'Handle Office', field: '', required: true },
            { title: 'Note', field: '' },
        ];

        if (this.isUpdate) {
            this.headers.push({ title: 'Receipt No', field: '', required: true },
                { title: 'Creator', field: '', required: true },
                { title: 'Modified Time', field: '', required: true });
        } else {
            this._store.select(getCurrentUserState)
                .pipe(takeUntil(this.ngUnsubscribe))
                .subscribe((user: SystemInterface.IClaimUser) => {
                    this.currentOffice = user.officeId;
                });
        }

        this._store.select(ReceiptCombineExchangeState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((exchange: number) => { this.exchangeRate = exchange });

        this._store.select(getCurrentUserState)
            .pipe(
                filter(c => !!c.userName),
                tap((c) => this.currentUser = c),
                switchMap((currentUser: SystemInterface.IClaimUser | any) => {
                    if (!!currentUser.userName) {
                        return forkJoin([
                            this._systemRepo.getOfficePermission(currentUser.id, currentUser.companyId),
                            this._catalogueRepo.getListPartner(null, null, {
                                active: true,
                                partnerMode: 'Internal',
                                notEqualInternalCode: currentUser.internalCode
                            })
                        ]).pipe(map(([offices, partners]) => ({ offices: offices, obhPartners: partners })))
                    }
                }),
                takeUntil(this.ngUnsubscribe),
            )
            .subscribe((data: { offices: any[], obhPartners: any[] }) => {
                console.log(data);
                this.offices = data.offices;
                this.obhPartners = data.obhPartners;
            })

        // * Listen select partner event from Redux Store.
        this._actionStoreSubject
            .pipe(
                filter(x => x.type === ReceiptCombineActionTypes.SELECT_PARTNER_RECEIPT_COMBINE),
                switchMap((data: {
                    id: string,
                    shortName: string,
                    accountNo: string,
                    partnerNameEn: string,
                    type: string,
                    salemanId: string,
                }) => {
                    return this._catalogueRepo.getACRefPartnerWithSaleman(data.id, data.salemanId, CommonEnum.PartnerGroupEnum.AGENT);//.pipe(map(values => [data, ...values])) // * Khởi tạo giá trị là partner đang chọn.
                }),
                skipWhile((v) => v.length === 0),
                filter(value => !!value.length),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    this.partners = data;
                }
            )
    }

    duplicateGeneralItem(index: number) {
        const newItem = cloneDeep(this.generalReceipts[index]);
        newItem.duplicate = false;
        newItem.receiptNo = null;
        newItem.userCreated = null;
        newItem.datetimeModified = null;
        this._store.dispatch(AddGeneralCombineToReceipt({ generalCombineList: [newItem] }));
    }

    deleteGeneralItem(index: number) {
        this.isSubmitted = false;
        this.generalReceipts.splice(index, 1);

    }

    addGeneralItem() {
        this._store.select(ReceiptCombinePartnerState)
            .subscribe((partner: any) => {
                this.partnerId = partner;
            })
            let salemanId=null;
            this._store.select(ReceiptCombineSalemanState)
            .subscribe((data: any) => {
                salemanId = data;
            })
        if (!this.partnerId) {
            this._store.dispatch(AddGeneralCombineToReceipt({ generalCombineList: [] }));
            return;
        }

        const partner = this.partners.find(x => x.id === this.partnerId);
        if(!partner){
            this._toastService.warning('Parent partner doesn\'t have any contract information. Please check again!');
            return;
        }
        let newItem: GeneralCombineReceiptModel[] = [{
            id: SystemConstants.EMPTY_GUID,
            partnerId: partner.id,
            paymentMethod: null,
            officeId: this.currentOffice,
            amountUsd: null,
            amountVnd: null,
            obhPartnerId: null,
            duplicate: false,
            isModified: true,
            agreementId: partner.contractId,
            receiptNo: null,
            userCreated: null,
            datetimeModified: null
        }];
        this._store.dispatch(AddGeneralCombineToReceipt({ generalCombineList: newItem }));
    }

    onSelectDataTableInfo(data: any, generalReceiptItem: GeneralCombineReceiptModel, key: string) {
        switch (key) {
            case 'amountUsd':
                const amountVnd = +(+data * this.exchangeRate).toFixed(0) || 0;
                generalReceiptItem.amountVnd = amountVnd;
                break;
            case 'partnerId':
                generalReceiptItem.partnerId = data.id;
                // generalReceiptItem.agreementId = data.contractId;
                break;
            default:
                generalReceiptItem[key] = data;
                break;
        }
    }

    confirmUpdateReceipt(type: string, action: string, data: any) {
        this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
            body: 'Do you want to save receipt ' + data.receiptNo + '?',
            title: 'Alert',
            labelCancel: 'No',
            labelConfirm: 'Yes',
            iconConfirm: 'la la-save'
        }, () => {
            this.onSaveReceiptGroup(type, action, data);
        })
    }
    
    onSaveReceiptGroup(type: string, action: string, data: any = null) {
        if (action === 'draft') {
            this.onSaveReceipt.emit({ type: type, action: !!this.generalReceipts.length ? 'draft' : 'update', receipt: data });
        } else {
            this.onSaveReceipt.emit({ type: type, action: action, receipt: data });
        }
    }

    checkAllowDelete(data: any, index: number) {
        if (!this.isUpdate) {
            this._store.dispatch(RemoveDebitCombine({ indexGrp: 0, index: index, _typeList: 'general'}));
        } 
        // else {
        //     this._accountingRepo
        //         .checkAllowDeleteCusPayment(data.id)
        //         .subscribe((value: boolean) => {
        //             if (value) {
        //                 const messageDelete = `Do you want to delete Receipt ${data.receiptNo} ? `;
        //                 this.showPopupDynamicRender(ConfirmPopupComponent, this.viewContainer.viewContainerRef, {
        //                     title: 'Delete Receipt',
        //                     body: messageDelete,
        //                     labelConfirm: 'Yes',
        //                     classConfirmButton: 'btn-danger',
        //                     iconConfirm: 'la la-trash',
        //                     center: true
        //                 }, () => this.onSaveReceiptGroup('general', 'delete', data));
        //             } else {
        //                 this.showPopupDynamicRender(Permission403PopupComponent, this.viewContainer.viewContainerRef, { center: true });
        //             }
        //         });
        // }
    }
}
