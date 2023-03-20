import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from "@angular/core";
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { template } from "lodash";
import { DocumentationRepo, OperationRepo } from "@repositories";
import { InjectViewContainerRefDirective } from "@directives";
import { PagerSetting } from "src/app/shared/models/layout/pager-setting.model";
import { PAGINGSETTING } from "src/constants/paging.const";
import { CustomClearanceFormSearchComponent } from "../components/form-search-custom-clearance/form-search-custom-clearance.component";
import { ModalDirective } from "ngx-bootstrap/modal";
import { finalize } from "rxjs/operators";
import { BehaviorSubject } from "rxjs";


@Component({
    selector: 'get-custom-clearance-from-Ecus',
    templateUrl: './get-custom-clearance-from-Ecus.html'
})
export class CustomClearanceFromEcus extends PopupBase implements OnInit {
    @ViewChild('staticModal') public staticModal:ModalDirective;
    @ViewChild(CustomClearanceFormSearchComponent) CustomClearanceComponent: CustomClearanceFormSearchComponent;
    @ViewChild(InjectViewContainerRefDirective) viewContainerRef: InjectViewContainerRefDirective;
    @Input() currentJob: OpsTransaction;
    @Output() isCloseModal = new EventEmitter();
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

    form: FormGroup;
    headers: CommonInterface.IHeaderTable[];
    customNo: AbstractControl;
    strKeySearch: string= "";
    pager: PagerSetting = PAGINGSETTING;
    isShow: boolean = false;
    sort: string = null;
    term$ = new BehaviorSubject<string>('');;
    checkAllNotImported = false;
    notImportedCustomClearances: any[] = [];
    partnerTaxcode = '';
    constructor(
        private _fb: FormBuilder,
        private _sortService: SortService,
        private _documentationRepo: DocumentationRepo,
        private _operationRepo: OperationRepo) {
        super();
    }
    
    ngOnInit() {
        this.initForm();
        this.pager.totalItems = 15;
    }


    initForm() {
        this.form = this._fb.group({
            customNo: ['', Validators.compose([
                Validators.required
            ])]
        });
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
    }

    onSearchRequest(){
        console.log("Hello");
    }

    onSearchAutoComplete(keyword: string) {
        this.term$.next(keyword.trim());
        if (this.customNo.value === '') {
            this.getListCleranceNotImported();
        }
    }
    
    
    changeAllNotImported() {
        if (this.checkAllNotImported) {
            this.notImportedCustomClearances.forEach(x => {
                x.isChecked = true; 
            });
        } else {
            this.notImportedCustomClearances.forEach(x => {
                x.isChecked = false;
            });
        }
    }

    getListCleranceNotImported() {
        this.notImportedCustomClearances = [];
        if (this.customNo.value !== '') {
            this.strKeySearch = this.customNo.value;
        } else {
            this.getClearanceNotImported();
        }
    }
    
    getClearanceNotImported() {
        console.log(this.partnerTaxcode);
        this._operationRepo.getListNotImportToJob(this.strKeySearch, this.partnerTaxcode, false, this.page, this.pageSize)
            .pipe(
                finalize(() => {
                    this.changeAllNotImported();
                    this.isLoading = false;
                })
            )
            .subscribe(
                (res: any) => {
                    this.notImportedCustomClearances = res.data || [];
                    this.totalItems = res.totalItems;
                }
            );
    }

    close() {
        this.hide();
    }
}

