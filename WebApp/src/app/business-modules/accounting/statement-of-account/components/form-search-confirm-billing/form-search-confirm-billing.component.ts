import { formatDate } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { JobConstants, SystemConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';

@Component({
    selector: 'form-search-confirm-billing',
    templateUrl: './form-search-confirm-billing.component.html'
})

export class ConfirmBillingFormSearchComponent extends AppForm implements OnInit {
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

    searchOption: AbstractControl;
    referenceNo: AbstractControl;
    partner: AbstractControl;
    dateType: AbstractControl;
    issueDate: AbstractControl;
    confirmedBilling: AbstractControl;
    service: AbstractControl;
    csHandling: AbstractControl;

    partners: Observable<Partner[]>;
    formSearch: FormGroup;

    displayFieldsPartner: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    displayFieldsCreator: CommonInterface.IComboGridDisplayField[] = [
        { field: 'username', label: 'User Name' },
        { field: 'employeeNameVn', label: 'Full Name' }
    ];

    searchOptionList: any[] = [
        { id: 'Debit Note', text: 'Debit Note' },
        { id: 'SOA', text: 'SOA' },
        { id: 'VAT Invoice', text: 'VAT Invoice' }
    ];
    selectedSearchOption: any[] = [this.searchOptionList[0]];
    dateTypeList: any[] = [
        { id: 'Confirm Billing Date', text: 'Confirm Billing Date' },
        { id: 'VAT Invoice Date', text: 'VAT Invoice Date' }
    ];
    confirmedBillingList: any[] = [
        { id: 'All', text: 'All' },
        { id: 'Yes', text: 'Yes' },
        { id: 'No', text: 'No' }
    ];

    services: any[] = [];
    selectedService: any[] = [];
    csHandlings: any[] = [];
    selectedCsHandling: any[] = [];

    dataSearch: any;
    userLogged: User;

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _sysRepo: SystemRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
    ) {
        super();
        this.requestReset = this.reset;
    }

    ngOnInit() {
        this.initFormSearch();
        this.loadPartnerList();
        this.loadUserList();
        this.getService();
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            searchOption: [this.selectedSearchOption],
            referenceNo: [],
            partner: [],
            dateType: [[this.dateTypeList[1]]],
            issueDate: [], // [{ startDate: new Date(), endDate: new Date() }],
            confirmedBilling: [[this.confirmedBillingList[0]]],
            service: [this.selectedService],
            csHandling: [this.selectedCsHandling]
        });
        this.searchOption = this.formSearch.controls['searchOption'];
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.partner = this.formSearch.controls['partner'];
        this.dateType = this.formSearch.controls['dateType'];
        this.issueDate = this.formSearch.controls['issueDate'];
        this.confirmedBilling = this.formSearch.controls['confirmedBilling'];
        this.service = this.formSearch.controls['service'];
        this.csHandling = this.formSearch.controls['csHandling'];

        this.searchOption.setValue(this.selectedSearchOption);
    }

    getService() {
        this._sysRepo.getListServiceByPermision()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.sortIncreaseServices('text', true);
                        this.services.unshift({ id: 'All', text: 'All' });
                        this.selectedService = [this.services[0]];
                    } else {
                        this.handleError(null, (data) => {
                            this._toastService.error(data.message, data.title);
                        });
                    }
                },
            );
    }

    sortIncreaseServices(sortField: string, order: boolean) {
        this.services = this._sortService.sort(this.services, sortField, order);
    }

    loadPartnerList() {
        this.partners = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, true);
    }

    loadUserList() {
        this._systemRepo.getSystemUsers({}).pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.csHandlings = this.utility.prepareNg2SelectData(res, 'id', 'username');
                        this.sortIncreaseServices('username', true);
                        this.csHandlings.unshift({ id: 'All', text: 'All' });
                        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
                        this.selectedCsHandling = [this.csHandlings.filter(stf => stf.id === this.userLogged.id)[0]];
                        this.csHandling.setValue(this.selectedCsHandling);
                    } else {
                        this.handleError(null, (data) => {
                            this._toastService.error(data.message, data.title);
                        });
                    }
                },
            );
    }

    onSelectDataFormInfo(data: { id: any; }, type: string) {
        console.log(data);
        switch (type) {
            case 'partner':
                this.formSearch.controls['partner'].setValue(data.id);
                break;
            case 'service':
                if (data.id === 'All') {
                    this.selectedService = [];
                    this.selectedService.push({ id: 'All', text: "All" });
                } else {
                    this.selectedService.push(data);
                    this.detectServiceWithAllOption('service', data);
                    this.service.setValue(this.selectedService);
                }
                break;
            case 'csHandling':
                if (data.id === 'All') {
                    this.selectedCsHandling = [];
                    this.selectedCsHandling.push({ id: 'All', text: "All" });
                } else {
                    this.selectedCsHandling.push(data);
                    this.detectServiceWithAllOption('csHandling', data);
                    this.csHandling.setValue(this.selectedCsHandling);
                }
                break;
            default:
                break;
        }
    }

    onRemoveDataFormInfo(data: any, type: string) {
        if (type === 'service') {
            this.selectedService.splice(this.selectedService.findIndex((item) => item.id === data.id), 1);
        }
        if (type === 'csHandling') {
            this.selectedCsHandling.splice(this.selectedCsHandling.findIndex((item) => item.id === data.id), 1);
        }
        this.detectServiceWithAllOption(type);
    }

    detectServiceWithAllOption(type: string, data?: any) {
        if (type === "service") {
            if (!this.selectedService.every((value) => value.id !== 'All')) {
                this.selectedService.splice(this.selectedService.findIndex((item) => item.id === 'All'), 1);
                this.selectedService = [];
                this.selectedService.push(data);
            }
        }
        if (type === "csHandling") {
            if (!this.selectedCsHandling.every((value) => value.id !== 'All')) {
                this.selectedCsHandling.splice(this.selectedCsHandling.findIndex((item) => item.id === 'All'), 1);
                this.selectedCsHandling = [];
                this.selectedCsHandling.push(data);
            }
        }
    }

    mapObject(dataSelected: any[], dataList: any[]) {
        let result = '';
        if (dataSelected.length > 0) {
            if (dataSelected[0].id === 'All') {
                const list = dataList.filter(f => f.id !== 'All');
                result = list.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            } else {
                result = dataSelected.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            }
        }
        return result;
    }

    onSubmit() {
        const criteria: any = {
            searchOption: this.searchOption.value[0].id,
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: this.partner.value,
            dateType: !!this.dateType.value ? this.dateType.value[0].id : null,
            fromDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            toDate: this.issueDate.value ? (this.issueDate.value.endDate !== null ? formatDate(this.issueDate.value.endDate, 'yyyy-MM-dd', 'en') : null) : null,
            confirmedBilling: !!this.confirmedBilling.value ? this.confirmedBilling.value[0].id : null,
            services: this.mapObject(this.selectedService, this.services),
            csHandling: this.mapObject(this.selectedCsHandling, this.csHandlings)
        };
        console.log(criteria);
        this.onSearch.emit(criteria);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        // this.issueDate.setValue({ startDate: new Date(), endDate: new Date() });
        this.selectedSearchOption = [this.searchOptionList[0]];
        this.searchOption.setValue(this.selectedSearchOption);

        this.dateType.setValue([this.dateTypeList[1]]);

        this.confirmedBilling.setValue([this.confirmedBillingList[0]]);

        this.selectedService = [this.services[0]];
        this.service.setValue(this.selectedService);

        this.selectedCsHandling = [this.csHandlings.filter(stf => stf.id === this.userLogged.id)[0]];
        this.csHandling.setValue(this.selectedCsHandling);

        this.onSearch.emit(<any>{
            searchOption: this.searchOption.value[0].id,
            dateType: !!this.dateType.value ? this.dateType.value[0].id : null,
            fromDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            toDate: this.issueDate.value ? (this.issueDate.value.endDate !== null ? formatDate(this.issueDate.value.endDate, 'yyyy-MM-dd', 'en') : null) : null,
            confirmedBilling: !!this.confirmedBilling.value ? this.confirmedBilling.value[0].id : null,
            services: this.mapObject(this.selectedService, this.services),
            csHandling: this.mapObject(this.selectedCsHandling, this.csHandlings)
        });
    }
}
