import { formatDate } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup } from '@angular/forms';
import { AccountingConstants, JobConstants } from '@constants';
import { CommonEnum } from '@enums';
import { Partner, User } from '@models';
import { Store } from '@ngrx/store';
import { CatalogueRepo, SystemRepo } from '@repositories';
import { Observable } from 'rxjs';
import { takeUntil, catchError } from 'rxjs/operators';
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

    searchOptionList: string[] = ['Debit Note', 'SOA', 'VAT Invoice'];
    dateTypeList: string[] = ['Confirm Billing Date', 'VAT Invoice Date'];
    confirmedBillingList: string[] = ['All', 'Yes', 'No'];

    services: any[] = [];
    selectedService: any[] = [];
    csHandlings: any[] = [];
    selectedCsHandlings: any[] = [];

    dataSearch: any;

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
            searchOption: [],
            referenceNo: [],
            partner: [],
            dateType: [],
            issueDate: [{ startDate: new Date(), endDate: new Date() }],
            confirmedBilling: [],
            service: [],
            csHandling: []
        });
        this.searchOption = this.formSearch.controls['searchOption'];
        this.referenceNo = this.formSearch.controls['referenceNo'];
        this.partner = this.formSearch.controls['partner'];
        this.dateType = this.formSearch.controls['dateType'];
        this.issueDate = this.formSearch.controls['issueDate'];
        this.confirmedBilling = this.formSearch.controls['confirmedBilling'];
        this.service = this.formSearch.controls['service'];
        this.csHandling = this.formSearch.controls['csHandling'];
    }

    getService() {
        this._sysRepo.getListServiceByPermision()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {

                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        //
                        // sort A -> Z theo text services 
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
                        //
                        // sort A -> Z theo text services 
                        this.sortIncreaseServices('text', true);

                        this.csHandlings.unshift({ id: 'All', text: 'All' });

                        this.selectedCsHandlings = [this.csHandlings[0]];
                    } else {
                        this.handleError(null, (data) => {
                            this._toastService.error(data.message, data.title);
                        });
                    }
                },
            );
    }

    onSelectDataFormInfo(data: { id: any; }, type: string) {
        switch (type) {
            case 'partner':
                this.formSearch.controls['partner'].setValue(data.id);
                break;
            case 'service':
                this.formSearch.controls['creator'].setValue(data.id);
                break;
            case 'csHandling':
                this.formSearch.controls['creator'].setValue(data.id);
                break;
            default:
                break;
        }
    }

    onSubmit() {
        const criteria: any = {
            searchOption: this.searchOption.value,
            referenceNos: !!this.referenceNo.value ? this.referenceNo.value.trim().replace(/(?:\r\n|\r|\n|\\n|\\r)/g, ',').trim().split(',').map((item: any) => item.trim()) : null,
            partnerId: this.partner.value,
            dateType: this.dateType.value,
            fromDate: this.issueDate.value ? (this.issueDate.value.startDate !== null ? formatDate(this.issueDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
            toDate: this.issueDate.value ? (this.issueDate.value.endDate !== null ? formatDate(this.issueDate.value.endDate, 'yyyy-MM-dd', 'en') : null) : null,
            confirmedBilling: this.confirmedBilling.value,
            services: this.service.value,
            csHandling: this.csHandling.value
        };
        console.log(criteria);
        this.onSearch.emit(criteria);
    }

    search() {
        this.onSubmit();
    }

    reset() {
        this.formSearch.reset();
        this.issueDate.setValue({ startDate: new Date(), endDate: new Date() });
    }
}
