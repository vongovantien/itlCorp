import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { template } from "lodash";
import { DocumentationRepo, OperationRepo } from "@repositories";
import { PagerSetting } from "src/app/shared/models/layout/pager-setting.model";
import { PAGINGSETTING } from "src/constants/paging.const";
import { CustomClearanceFormSearchComponent } from "../components/form-search-custom-clearance/form-search-custom-clearance.component";
import { ModalDirective } from "ngx-bootstrap/modal";
import { debounceTime, distinctUntilChanged, finalize, takeUntil} from "rxjs/operators";
import { BehaviorSubject, Observable } from "rxjs";
import { ToastrService } from "ngx-toastr";

@Component({
    selector: 'get-custom-clearance-from-Ecus',
    templateUrl: './get-custom-clearance-from-Ecus.html'
})
export class CustomClearanceFromEcus extends PopupBase implements OnInit {
    @ViewChild('staticModal') public staticModal: ModalDirective;
    @ViewChild(CustomClearanceFormSearchComponent) CustomClearanceComponent: CustomClearanceFormSearchComponent;
    // @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @Input() currentJob: OpsTransaction;
    @Input() customer: CustomClearanceFormSearchComponent;
    @Output() isCloseModal = new EventEmitter();
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

    form: FormGroup;
    headers: CommonInterface.IHeaderTable[];
    customNo: AbstractControl;
    requestList: any = null;
    strKeySearch: string = "";
    pager: PagerSetting = PAGINGSETTING;
    isShow: boolean = false;
    sort: string = null;
    term$ = new BehaviorSubject<string>('');
    dataEcus: any;
    ;
    checkAllNotImported = false;
    partnerTaxcode = '';
    constructor(
        private _fb: FormBuilder,
        private _sortService: SortService,
        private _toastrService: ToastrService,
        private _documentationRepo: DocumentationRepo,
        private _operationRepo: OperationRepo) {
        super();
        this.requestList = this.getClearanceNotImported;
    }

    ngOnInit() {
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
        this.term$.pipe(
            debounceTime(2000),
            distinctUntilChanged(),
            takeUntil(this.ngUnsubscribe)
        ).subscribe((text: string) => {
            this.getClearanceNotImported();
        });
    }


    initForm() {
        this.pageSize=30;
        this.form = this._fb.group({
            customNo: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.customNo = this.form.controls['customNo'];
    }
    onSearchAutoComplete(keyword: string) {
        console.log(this.customNo);
        this.term$.next(keyword.trim());
        if (this.customNo.value === '') {
            this.getListCleranceNotImported();
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
        if (this.customNo.value !== '') {
            this.strKeySearch = this.customNo.value;
        } else {
            this.getClearanceNotImported();
        }
    }

    getClearanceNotImported() {
        this._operationRepo.getUserCustomClearance(this.page, this.pageSize).pipe(
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
        this.customNo.setValue('');
        this.strKeySearch = '';
        this._operationRepo.getUserCustomClearance(this.page, this.pageSize);
    }

    updateJobToClearance() {
        const dataToUpdate = this.dataEcus.filter(x => x.isChecked === true);
        console.log(dataToUpdate);
        if (dataToUpdate.length > 0) {
            this._operationRepo.importCustomClearance(dataToUpdate)
                .pipe(finalize(() => this.hide()))
                .subscribe(
                    (responses: CommonInterface.IResult | any) => {
                        if (!!responses.message) {
                            this._toastrService.success(responses.message.value, '');
                        }
                    }
                );
        }
    }

    close() {
        this.keyword = '';
        this.page = 1;
        this.pageSize = this.numberToShow[2];
        this.customNo.setValue('');
        this.strKeySearch = '';
        this._operationRepo.getUserCustomClearance(this.page, this.pageSize);
        this.hide();
    }
}
