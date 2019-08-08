import { Component, ViewChild } from '@angular/core';
import { StatementOfAccountAddChargeComponent } from '../components/poup/add-charge/add-charge.popup';
import { NgxSpinnerService } from 'ngx-spinner';
import { AccoutingRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SOA, SOASearchCharge, Charge } from 'src/app/shared/models';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { ActivatedRoute, Router } from '@angular/router';
import { AppList } from 'src/app/app.list';
import { SortService } from 'src/app/shared/services';
import { forkJoin } from 'rxjs';
import { formatDate } from '@angular/common';
import moment from 'moment';
@Component({
    selector: 'app-statement-of-account-edit',
    templateUrl: './edit-soa.component.html',
})
export class StatementOfAccountEditComponent extends AppList {
    @ViewChild(StatementOfAccountAddChargeComponent, { static: false }) addChargePopup: StatementOfAccountAddChargeComponent;

    currencyList: any[];
    selectedCurrency: any = null;

    selectedRange: any;

    soa: SOA = new SOA();
    soaNO: string = '';
    currencyLocal: string = '';

    headers: CommonInterface.IHeaderTable[] = [];
    isCheckAllCharge: boolean = false;

    dataSearch: SOASearchCharge;

    charges: Charge[] = [];
    configCharge: CommonInterface.IComboGirdConfig = {
        placeholder: 'Please select',
        displayFields: [],
        dataSource: [],
        selectedDisplayFields: [],
    };

    constructor(
        private _spinner: NgxSpinnerService,
        private _accoutingRepo: AccoutingRepo,
        private _toastService: ToastrService,
        private _activedRoute: ActivatedRoute,
        private _sysRepo: SystemRepo,
        private _sortService: SortService,
        private _router: Router
    ) {
        super();
        this.requestList = this.sortChargeList;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Charge Code', field: 'chargeCode', sortable: true },
            { title: 'Charge Name', field: 'chargeName', sortable: true },
            { title: 'JobID', field: 'jobId', sortable: true },
            { title: 'HBL', field: 'hbl', sortable: true },
            { title: 'MBL', field: 'mbl', sortable: true },
            { title: 'Custom No', field: 'customNo', sortable: true },
            { title: 'Debit', field: 'debit', sortable: true },
            { title: 'Credit', field: 'credit', sortable: true },
            { title: 'Currency', field: 'currency', sortable: true },
            { title: 'Invoice No', field: 'invoiceNo', sortable: true },
            { title: 'Services Date', field: 'serviceDate', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];
        this._activedRoute.queryParams.subscribe((params: any) => {
            if (!!params.no && !!params.currency) {
                this.soaNO = params.no;
                this.currencyLocal = params.currency;
                // this.getCurrency();
                // this.getListCharge()
                this.getDetailSOA(this.soaNO, this.currencyLocal);
            }
        });
    }

    addCharge() {
        this.addChargePopup.show();
    }

    getDetailSOA(soaNO: string, currency: string) {
        this._spinner.show();
        forkJoin([
            this._accoutingRepo.getDetaiLSOA(soaNO, currency),
            this._sysRepo.getListCurrency(),
            this._sysRepo.getListCharge()

        ]).pipe(
            catchError(this.catchError),
            finalize(() => { this._spinner.hide(); })
        ).subscribe(
            ([dataSoa, dataCurrency, datCharge]: any[]) => {
                this.soa = new SOA(dataSoa);
                this.currencyList = dataCurrency;

                // * make all chargeshipment was selected
                for (const item of this.soa.chargeShipments) {
                    item.isSelected = this.isCheckAllCharge;
                }

                // * update currency
                this.selectedCurrency = !!this.soa.currency ? this.currencyList.filter((currencyItem: any) => currencyItem.id === this.soa.currency.trim())[0] : this.currencyList[0];

                // * update range Date
                this.selectedRange = { startDate: new Date(this.soa.soaformDate), endDate: new Date(this.soa.soatoDate) };

                // * Update charge 
                this.charges = datCharge;
                this.charges.push(new Charge({ code: 'All', id: 'All', chargeNameEn: 'All' }));

                this.configCharge.dataSource = datCharge || [];
                this.configCharge.displayFields = [
                    { field: 'code', label: 'Charge Code' },
                    { field: 'chargeNameEn', label: 'Charge Name EN ' },
                ];
                this.configCharge.selectedDisplayFields = ['code'];

                // * Update dataSearch for Add More Charge.
                const datSearchMoreCharge: SOASearchCharge = {
                    currency: this.soa.currency,
                    currencyLocal: 'VND', // TODO get currency local from user,
                    customerID: this.soa.customer,
                    dateType: this.soa.dateType,
                    toDate: this.soa.soatoDate,
                    fromDate: this.soa.soaformDate,
                    type: this.soa.type,
                    isOBH: this.soa.obh,
                    inSoa: false,
                    strCharges: this.soa.surchargeIds,
                    strCreators: this.soa.creatorShipment,
                    serviceTypeId: this.soa.serviceTypeId,
                    chargeShipments: this.soa.chargeShipments,
                    note: this.soa.note
                };
                this.dataSearch = new SOASearchCharge(datSearchMoreCharge);

            },
            (errors: any) => {
                this.handleError(errors);
            },
            () => { }
        );
    }


    getCurrency() {
        this._sysRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (dataCurrency: any) => {
                    this.currencyList = dataCurrency;
                },
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => { }
            );
    }
    
    getListCharge() {
        this._sysRepo.getListCharge()
            .pipe(catchError(this.catchError))
            .subscribe((data) => {
                this.charges = data;
                this.charges.push(new Charge({ code: 'All', id: 'All', chargeNameEn: 'All' }));

                this.configCharge.dataSource = data || [];
                this.configCharge.displayFields = [
                    { field: 'code', label: 'Charge Code' },
                    { field: 'chargeNameEn', label: 'Charge Name EN ' },
                ];
                this.configCharge.selectedDisplayFields = ['code'];
            },
                (errors: any) => {
                    this.handleError(errors);
                },
                // complete
                () => { }
            );
    }

    handleError(errors: any) {
        let message: string = 'Has Error Please Check Again !';
        let title: string = '';
        if (errors instanceof HttpErrorResponse) {
            message = errors.message;
            title = errors.statusText;
        }
        this._toastService.error(message, title, { positionClass: 'toast-bottom-right' });
    }


    sortChargeList(sortField?: string, order?: boolean) {
        this.soa.chargeShipments = this._sortService.sort(this.soa.chargeShipments, sortField, order);
    }

    onChangeCheckBoxCharge($event: Event) {
        this.isCheckAllCharge = this.soa.chargeShipments.every((item: any) => item.isSelected);
    }

    checkUncheckAllCharge() {
        for (const charge of this.soa.chargeShipments) {
            charge.isSelected = this.isCheckAllCharge;
        }
    }

    removeCharge() {
        this.soa.chargeShipments  = this.soa.chargeShipments.filter ( (item: any) => !item.isSelected);
    }

    back() {
        this._router.navigate(['home/accounting/statement-of-account']);
    }

    onUpdateMoreSOA(data: any) {
        this.soa.chargeShipments = data.chargeShipments;
        this.isCheckAllCharge = false;
    }

    updateSOA() {
        /*
        * endDate must greater or equal soaToDate
                    * and 
        * fromDate must less or equal soaFromDate
        */
        if (!(moment(this.soa.soaformDate).isSameOrAfter(moment(this.selectedRange.startDate), 'day') && moment(this.selectedRange.endDate).isSameOrAfter(moment(this.soa.soatoDate), 'day'))) {
            this._toastService.warning(`Range date invalid `, '', { positionClass: 'toast-bottom-right' });
            return;
        }
        if (!this.soa.chargeShipments.length) {
            this._toastService.warning(`SOA Don't have any charges in this period, Please check it again! `, '', { positionClass: 'toast-bottom-right' });
            return;
        } else {
            const body = {
                surchargeIds: this.soa.chargeShipments.map((item: any) => item.id),
                id: this.soa.id,
                soano: this.soaNO,
                soaformDate: !!this.selectedRange.startDate ? formatDate(this.selectedRange.startDate, 'yyyy-MM-dd', 'en') : null,
                soatoDate: !!this.selectedRange.endDate ? formatDate(this.selectedRange.endDate, 'yyyy-MM-dd', 'en') : null,
                currency: this.selectedCurrency.id,
                status: this.soa.status,
                note: this.soa.note,
                userCreated: this.soa.userCreated,
                userModified: this.soa.userModified,
                datetimeCreated: this.soa.datetimeCreated,
                datetimeModified: this.soa.datetimeModified,
                serviceTypeId: this.soa.serviceTypeId,
                dateType: this.soa.dateType,
                type: this.soa.type,
                obh: this.soa.obh,
                creatorShipment: this.soa.creatorShipment,
                customer: this.soa.customer
            };
            this._spinner.show();

            this._accoutingRepo.updateSOA(body)
                .pipe(
                    catchError(this.catchError),
                    finalize(() => { this._spinner.hide(); })
                )
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(`SOA ${res.data.soano} is successfull`, 'Update Success', { positionClass: 'toast-bottom-right' });

                            // * get detail again
                            this.getDetailSOA(this.soaNO, this.currencyLocal);

                            // * init checkbox all
                            this.isCheckAllCharge = false;
                        }
                    },
                    (errors: any) => {
                        this.handleError(errors);
                    },
                    () => { }
                );
        }
    }

  

    addMoreCharge() {
        this.addChargePopup.searchInfo = this.dataSearch;
        this.addChargePopup.getListShipmentAndCDNote(this.dataSearch);

        this.addChargePopup.charges = this.charges;
        this.addChargePopup.configCharge = this.configCharge;

        this.addChargePopup.show({ backdrop: 'static' });
    }
}
