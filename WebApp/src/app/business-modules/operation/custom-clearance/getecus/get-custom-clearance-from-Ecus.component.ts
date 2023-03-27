import { Component, EventEmitter, OnInit, Output, ViewChild } from "@angular/core";
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { size, template } from "lodash";
import { DocumentationRepo, OperationRepo } from "@repositories";
import { PagerSetting } from "src/app/shared/models/layout/pager-setting.model";
import { PAGINGSETTING } from "src/constants/paging.const";
import { CustomClearanceFormSearchComponent } from "../components/form-search-custom-clearance/form-search-custom-clearance.component";
import { catchError, finalize} from "rxjs/operators";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'get-custom-clearance-from-Ecus',
    templateUrl: './get-custom-clearance-from-Ecus.html'
})
export class CustomClearanceFromEcus extends PopupBase implements OnInit {
    @ViewChild(CustomClearanceFormSearchComponent) CustomClearanceComponent: CustomClearanceFormSearchComponent;
    @Output() isCloseModal = new EventEmitter();

    form: FormGroup;
    headers: CommonInterface.IHeaderTable[];
    requestList: any = null;
    strKeySearch: string = "";
    pager: PagerSetting = PAGINGSETTING;
    dataEcus: any;
    checkAllNotImported = false;
    constructor(
        private _toastrService: ToastrService,
        private _sortService: SortService,
        private _operationRepo: OperationRepo) {
        super();
        this.requestList = this.getClearanceNotImported;
    }

    ngOnInit() {
        this.pageSize=this.numberToShow[2];
        this.headers = [
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'HBL No', field: 'hblNo', sortable: true },
            { title: 'Partner Name', field: 'partnerName', sortable: true },
            { title: 'Type', field: 'type', sortable: true },
            { title: 'Clearance Location', field: 'clearanceLocation', sortable: true },
            { title: 'Package Qty', field: 'pkgQty', sortable: true },
            { title: 'Cont Qty', field: 'contQty', sortable: true },
        ];
        this.initForm();
    }


    initForm() {
        console.log(this.pageSize);
        this.pageSize=this.numberToShow[2];
        console.log(this.pageSize);
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
        console.log(this.pageSize);
        this._operationRepo.getUserCustomClearance(this.page, this.pageSize)
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
        console.log(checkedData.length);
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
        this.keyword = '';
        this.page = 1;
        this.pageSize = this.numberToShow[2];
        this.strKeySearch = '';
        this._operationRepo.getUserCustomClearance(this.page, this.pageSize);
    }

    updateJobToClearance() {
        const dataToUpdate = this.dataEcus.filter(x => x.isChecked === true);
        if (dataToUpdate.length > 0) {
            this._operationRepo.importCustomClearance(dataToUpdate)
                .pipe(catchError(this.catchError), finalize(() => this.refreshData()))
                .subscribe(
                    (responses: CommonInterface.IResult | any) => {
                        if (!!responses.message) {
                            this._toastrService.success(responses.message.value, '');
                            this.dataEcus.filter(x => x.isChecked === true);
                        }
                    }
                );
        }
        else{
            this._toastrService.warning("Chưa chọn clearance để save!", '');
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
        this.strKeySearch = '';
        this.hide();
    }
}
