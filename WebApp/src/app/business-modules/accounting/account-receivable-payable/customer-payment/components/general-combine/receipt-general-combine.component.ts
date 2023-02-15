import { coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, Input, OnInit } from '@angular/core';
import { JobConstants } from '@constants';
import { Office, Partner } from '@models';
import { ActionsSubject, Store } from '@ngrx/store';
import { AccountingRepo, CatalogueRepo, SystemRepo } from '@repositories';
import { getCurrentUserState } from '@store';
import { cloneDeep, forEach, take } from 'lodash';
import { forkJoin, Observable, of } from 'rxjs';
import { filter, map, mergeMap, skipWhile, startWith, switchMap, switchMapTo, takeUntil, tap } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { ReceiptCombineActionTypes } from '../../store/actions';
import { ICustomerPaymentState, ReceiptCombineExchangeState, ReceiptCombineLoadingState, ReceiptCombinePartnerState } from '../../store/reducers';

@Component({
    selector: 'receipt-general-combine',
    templateUrl: './receipt-general-combine.component.html',
    styleUrls: ['./receipt-general-combine.component.scss']
})
export class ARCustomerPaymentReceiptGeneralCombineComponent extends AppList implements OnInit {

    @Input() set readOnly(val: any) {
        this._readonly = coerceBooleanProperty(val);
    }
    get readonly(): boolean {
        return this._readonly;
    }
    private _readonly: boolean = false;

    generalReceipts: IGeneralReceipt[] = [];
    partners: Partner[] = [];
    obhPartners: Partner[] = [];
    offices: Office[] = []
    paymentMethods: CommonInterface.ICommonTitleValue[] = [
        { title: 'Collect OBH Agency', value: 'Collect OBH Agency' },
        { title: 'Pay OBH Agency', value: 'Pay OBH Agency' },
        { title: 'Collected Amount', value: 'Collected Amount' },
        { title: 'Advance Agency', value: 'Advance Agency' },
        { title: 'Bank Fee Agency', value: 'Bank Fee Agency' },
        { title: 'Receive form Pay OBH', value: 'Receive form Pay OBH' },
        { title: 'Receive from Collect OBH', value: 'Receive from Collect OBH' },
    ]

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;
    isSubmitted: boolean = false;

    exchangeRate: number;

    constructor(
        private readonly _store: Store<ICustomerPaymentState>,
        private readonly _catalogueRepo: CatalogueRepo,
        private readonly _systemRepo: SystemRepo,
        private readonly _actionStoreSubject: ActionsSubject,
        private readonly _accountingRepo: AccountingRepo,
    ) {
        super();
    }

    ngOnInit(): void {
        this.isLoading = this._store.select(ReceiptCombineLoadingState);

        this.headers = [
            { title: 'Agency Name', field: '', width: 200, required: true },
            { title: 'Payment Method', field: '', required: true, width: 200 },
            { title: 'Amount', field: '', required: true },
            { title: 'Amount VND', field: '', required: true },
            { title: 'OBH Branch', field: '' },
            { title: 'Handle Office', field: '', required: true },
            { title: 'Note', field: '' },
        ];

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
                    type: string
                }) => {
                    return this._catalogueRepo.getSubListPartner(data.id).pipe(map(values => [data, ...values])) // * Khởi tạo giá trị là partner đang chọn.
                }),
                skipWhile((v) => v.length === 0),
                filter(value => !!value.length),
                takeUntil(this.ngUnsubscribe)
            )
            .subscribe(
                (data: any) => {
                    this.partners = data;
                    if (!!this.generalReceipts.length) {
                        this.generalReceipts.forEach(x => { x.partnerId = this.partners[0].id });
                    }
                }
            )
    }

    duplicateGeneralItem(index: number) {
        const newItem = cloneDeep(this.generalReceipts[index]);
        this.generalReceipts.push(newItem);
    }

    deleteGeneralItem(index: number) {
        this.isSubmitted = false;
        this.generalReceipts.splice(index, 1);

    }

    addGeneralItem() {
        const newItem: IGeneralReceipt = {
            paymentMethod: null,
            officeId: null,
            partnerId: null,
        };
        this.generalReceipts.push(newItem);
    }

    onSelectDataTableInfo(data: any, generalReceiptItem: IGeneralReceipt, key: string) {
        switch (key) {
            case 'amountUsd':
                const amountVnd = +(+data.target.value * this.exchangeRate).toFixed(0) || 0;
                generalReceiptItem.amountVnd = amountVnd;
                break;
            default:
                generalReceiptItem[key] = data;
                break;
        }
    }

}

interface IGeneralReceipt {
    partnerId?: string;
    paymentMethod?: string;
    amountVnd?: number;
    amountUsd?: number;
    obhPartnerId?: string;
    officeId?: string;
    notes?: string;
}
