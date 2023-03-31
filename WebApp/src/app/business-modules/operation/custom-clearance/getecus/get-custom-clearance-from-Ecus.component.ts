import { Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { FormGroup, FormBuilder, AbstractControl } from '@angular/forms';
import { OperationRepo } from "@repositories";
import { PagerSetting } from "src/app/shared/models/layout/pager-setting.model";
import { PAGINGSETTING } from "src/constants/paging.const";
import { CustomClearanceFormSearchComponent } from "../components/form-search-custom-clearance/form-search-custom-clearance.component";
import { catchError, finalize } from "rxjs/operators";
import { ToastrService } from "ngx-toastr";
import { Observable } from "rxjs";
import { Customer } from 'src/app/shared/models';
import { CommonEnum } from "@enums";
import { CatalogueRepo } from 'src/app/shared/repositories';
import { JobConstants } from "@constants";

@Component({
    selector: 'get-custom-clearance-from-Ecus',
    templateUrl: './get-custom-clearance-from-Ecus.html'
})
export class CustomClearanceFromEcus extends PopupBase implements OnInit {
    [x: string]: any;
    @ViewChild(CustomClearanceFormSearchComponent) CustomClearanceComponent: CustomClearanceFormSearchComponent;
    @Output() isCloseModal = new EventEmitter();

    form: FormGroup;
    formSearch: FormGroup;
    headers: CommonInterface.IHeaderTable[];
    requestList: any = null;
    pager: PagerSetting = PAGINGSETTING;
    dataEcus: any[] = [];
    filterTypes: CommonInterface.ICommonTitleValue[];
    clearanceNo: AbstractControl;
    filterType: AbstractControl;
    partnerId: AbstractControl;
    checkAllNotImported = false;

    customers: Observable<Customer[]>;
    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_PARTNER;

    constructor(
        private _fb: FormBuilder,
        private _toastrService: ToastrService,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private _operationRepo: OperationRepo) {
        super();
        this.requestList = this.getClearanceNotImported;
    }

    ngOnInit() {
        this.pageSize = this.numberToShow[2];

        this.headers = [
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'HBL No', field: 'hblNo', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Clearance Location', field: 'clearanceLocation', sortable: true },
            { title: 'Package Qty', field: 'pkgQty', sortable: true },
            { title: 'Cont Qty', field: 'contQty', sortable: true },
            { title: 'Status', field: 'imprtStts' },
        ];
        this.filterTypes = [
            { title: 'Custom No', value: 'clearanceNo' },
            { title: 'HBL No', value: 'hblNo' },
            { title: 'Partner Name', value: 'partnerName' },
        ];
        this.initForm();
        this.filterType.setValue(this.filterTypes[0]);
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER, null);
    }

    initForm() {
        this.pageSize = this.numberToShow[2];
        this.formSearch = this._fb.group({
            clearanceNo: "",
            'customer': [],
            partnerId: [null],
            filterType: [this.filterTypes[0]]
        });
        this.partnerId = this.formSearch.controls['partnerId'];
        this.clearanceNo = this.formSearch.controls['clearanceNo'];
        this.filterType = this.formSearch.controls['filterType'];

        this.customer = this.formSearch.controls['customer'];
    }

    onInputAutoSearch(keyword: string) {
        this.term$.next(keyword.trim());
        if (this.customNo.value === '') {
            this.getListCleranceNotImported();
        } else {
            this.popupSearchMultiple.customNoSearch = '';
        }
    }

    changeAllNotImported() {
        if (this.checkAllNotImported) {
            this.dataEcus.forEach(x => {
                x.isChecked = true;
            });
        } else {
            this.dataEcus.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    getListCleranceNotImported() {
        this.dataEcus = [];
        this.getClearanceNotImported();
    }

    getClearanceNotImported() {
        this._operationRepo.getUserCustomClearance(this.filterType.value.value, this.clearanceNo.value, this.page, this.pageSize)
            .pipe(
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (data: CommonInterface.IResponsePaging) => {
                    this.dataEcus = data.data || [];
                    this.totalItems = data.totalItems;
                }
            );
    }

    removeAllChecked() {
        this.checkAllNotImported = false;
        const checkedData = this.dataEcus.filter(x => x.isChecked === true);
        if (checkedData.length > 0) {
            for (let i = 0; i < checkedData.length; i++) {
                const index = this.dataEcus.indexOf(x => x.id === checkedData[i].id);
                if (index > -1) {
                    this.dataEcus[index] = true;
                }
            }
        }
    }

    refreshData() {
        this.page = 1;
        this.pageSize = this.numberToShow[2];
        this.formSearch.reset();
        this.getListCleranceNotImported();
    }

    updateJobToClearance() {
        const dataToUpdate = this.dataEcus.filter(x => x.isChecked === true);
        if (dataToUpdate.length > 0) {
            this._operationRepo.importCustomClearance(dataToUpdate)
                .pipe(catchError(this.catchError))
                .subscribe(
                    (responses: CommonInterface.IResult | any) => {
                        if (!!responses.message) {
                            this._toastrService.success(responses.message.value, '');
                            this.dataEcus.filter(x => x.isChecked).forEach(x => {
                                x.imprtStts = true;
                            });
                        }
                    }
                );
        }
        else {
            this._toastrService.warning("Chưa chọn clearance để save!", '');
            this.changeAllNotImported();
        }
    }

    onSelectDataFormInfo(data, type: string) {
        switch (type) {
            case 'partner':
                this.clearanceNo.setValue((data as Customer).shortName);
                break;
            default:
                break;
        }
    }

    sortChargeCdNote(sort: string): void {
        if (this.dataEcus) {
            this.dataEcus = this._sortService.sort(this.dataEcus, sort, this.order);
        }
    }

    close() {
        this.keyword = '';
        this.page = 1;
        this.pageSize = this.numberToShow[2];
        this.hide();
    }
}
