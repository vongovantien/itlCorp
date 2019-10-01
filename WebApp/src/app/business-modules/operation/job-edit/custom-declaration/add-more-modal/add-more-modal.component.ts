import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SortService, BaseService } from 'src/app/shared/services';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { takeUntil, debounceTime, switchMap, skip, distinctUntilChanged, finalize, tap } from 'rxjs/operators';
import { API_MENU } from 'src/constants/api-menu.const';
import { BehaviorSubject, Observable } from 'rxjs';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { FormGroup, AbstractControl, FormBuilder, Validators, FormControl } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { NgxSpinnerService } from 'ngx-spinner';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { SearchMultipleComponent } from '../components/search-multiple/search-multiple.component';
import { OperationRepo } from 'src/app/shared/repositories';

@Component({
    selector: 'app-add-more-modal',
    templateUrl: './add-more-modal.component.html'
})

export class AddMoreModalComponent extends PopupBase implements OnInit {
    @ViewChild(SearchMultipleComponent, { static: false }) popupSearchMultiple: SearchMultipleComponent;
    @Input() currentJob: OpsTransaction;
    @Output() isCloseModal = new EventEmitter();
    @Output() onSearch: EventEmitter<any> = new EventEmitter<any>();

    term$ = new BehaviorSubject<string>('');
    isShow: boolean = false;
    notImportedData: any[];
    page: number = 1;
    totalItems: number = 0;
    numberToShow: number[] = [3, 15, 30, 50];
    pageSize: number = this.numberToShow[1];
    pager: PagerSetting = PAGINGSETTING;
    sort: string = null;
    order: any = false;
    keyword: string = '';
    requestList: any = null;
    requestSort: any = null;
    notImportedCustomClearances: any[] = [];
    checkAllNotImported = false;
    headers: CommonInterface.IHeaderTable[];
    dataNotImportedSearch: any[];
    form: FormGroup;
    customNo: AbstractControl;
    selectedCustom: any = null;
    customNoSearch: string = '';
    strKeySearch: string = '';

    searchCustomButtonSetting: ButtonModalSetting = {
        dataTarget: "search-custom-modal",
        typeButton: ButtonType.add
    };
    constructor(
        private _fb: FormBuilder,
        private _sortService: SortService,
        private api_menu: API_MENU,
        private baseServices: BaseService,
        private _http: HttpClient,
        private spiner: NgxSpinnerService,
        private _operationRepo: OperationRepo) {
        super();

        this.requestSort = this.sortLocal;
        // this.requestList = this.getDataNotImported;
    }
    ngOnInit() {
        this.initForm();
        this.headers = [
            { title: 'Custom No', field: 'clearanceNo', sortable: true },
            { title: 'Import Date', field: 'datetimeCreated', sortable: true },
            { title: 'Clearance Date', field: 'clearanceDate', sortable: true },
            { title: 'HBL No', field: 'hblid', sortable: true },
            { title: 'Export Country', field: 'exportCountryCode', sortable: true },
            { title: 'Import Country', field: 'importCountryCode', sortable: true },
            { title: 'Commodity Code', field: 'commodityCode', sortable: true },
            { title: 'Qty', field: 'qtyCont', sortable: true },
            { title: 'Parentdoc', field: 'firstClearanceNo', sortable: true },
            { title: 'Note', field: 'note', sortable: true },
        ];

        this.term$.pipe(
            tap(
                () => {
                    this.isLoading = true;
                }
            ),

            distinctUntilChanged(),
            finalize(() => {
                this.isLoading = false;
            }),
            this.autocomplete(300, ((term: any) => this._operationRepo.getListNotImportToJob(this.customNo.value, this.currentJob.customerId, false, this.pager.currentPage, this.pager.pageSize)))
        ).subscribe(
            (res: any) => {
                this.notImportedCustomClearances = res.data || [];
                this.pager.totalItems = res.totalItems;
                this.pager.totalPages = res.totalPages;
            },
            (error: any) => { },
            () => { }
        );
    }
    initPager(): any {
        this.pager.totalItems = 0;
        this.pager.currentPage = 1;
    }

    sortLocal(sort: string): void {
        this.notImportedCustomClearances = this._sortService.sort(this.notImportedCustomClearances, sort, this.order);
    }
    async refreshData() {
        this.keyword = '';


        this.getListCleranceNotImported();
    }
    setPage(pager: PagerSetting) {
        this.pager.currentPage = pager.currentPage;
        this.pager.totalPages = pager.totalPages;
        this.pager.pageSize = pager.pageSize;
        this.notImportedCustomClearances = [];
        this.getListCleranceNotImported();
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
        const checkedData = this.notImportedCustomClearances.filter(x => x.isChecked === true);
        if (checkedData.length > 0) {
            for (let i = 0; i < checkedData.length; i++) {
                const index = this.notImportedData.indexOf(x => x.id === checkedData[i].id);
                if (index > -1) {
                    this.notImportedData[index] = true;
                }
            }
        }
    }


    initForm() {
        this.form = this._fb.group({
            customNo: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.customNo = this.form.controls['customNo'];
    }
    // searchClearanceNotImported(event) {
    //     const keySearch = this.keyword.trim().toLocaleLowerCase();
    //     if (keySearch !== null && keySearch.length < 1 && keySearch.length > 0) {
    //         return 0;
    //     }
    //     this.dataNotImportedSearch = this.notImportedData.filter(item => item.clearanceNo.includes(keySearch)
    //         || (item.hblid == null ? '' : item.hblid.toLocaleLowerCase()).includes(keySearch)
    //         || (item.exportCountryCode == null ? '' : item.exportCountryCode.toLocaleLowerCase()).includes(keySearch)
    //         || (item.importCountryCode == null ? '' : item.importCountryCode.toLocaleLowerCase()).includes(keySearch)
    //         || (item.commodityCode == null ? '' : item.commodityCode.toLocaleLowerCase()).includes(keySearch)
    //         || (item.firstClearanceNo == null ? '' : item.firstClearanceNo.toLocaleLowerCase()).includes(keySearch)
    //         || (item.qtyCont == null ? '' : item.qtyCont.toString()).includes(keySearch)
    //     );
    //     this.totalItems = this.dataNotImportedSearch.length;
    //     this.notImportedCustomClearances = this.dataNotImportedSearch.slice(0, (this.pageSize - 1));
    // }
    removeAllChecked() {
        this.checkAllNotImported = false;
        const checkedData = this.notImportedCustomClearances.filter(x => x.isChecked === true);
        if (checkedData.length > 0) {
            for (let i = 0; i < checkedData.length; i++) {
                const index = this.notImportedCustomClearances.indexOf(x => x.id === checkedData[i].id);
                if (index > -1) {
                    this.notImportedCustomClearances[index] = true;
                }
            }
        }
    }
    // getDataNotImported() {
    //     if (this.notImportedData != null) {
    //         this.totalItems = this.notImportedData.length;
    //         console.log(this.notImportedData);
    //         if (this.page > 1) {
    //             this.notImportedCustomClearances = this.notImportedData.slice(this.page * (this.pageSize), (this.page + 1) * this.pageSize);
    //         }
    //         else {
    //             this.notImportedCustomClearances = this.notImportedData.slice(this.page, (this.pageSize - 1));
    //         }

    //         /* this.notImportedCustomClearances = this.notImportedData.slice(this.page * (this.pageSize), (this.page + 1) * this.pageSize);*/
    //     }
    // }
    async updateJobToClearance() {
        const dataToUpdate = this.notImportedCustomClearances.filter(x => x.isChecked === true);
        if (dataToUpdate.length > 0) {
            dataToUpdate.forEach(x => {
                x.jobNo = this.currentJob.jobNo;
            });
            const responses = await this.baseServices.postAsync(this.api_menu.Operation.CustomClearance.updateToAJob, dataToUpdate, false, true);
            if (responses.success === true) {
                this.updateShipmentVolumn(dataToUpdate);
                this.isCloseModal.emit(true);
                this.hide();
            }
        }
    }
    onSearchAutoComplete(keyword: string) {
        this.pager.currentPage = 1;
        this.pager.pageSize = 15;
        this.notImportedCustomClearances = [];
        this.term$.next(keyword.trim());
        if (keyword.trim() == '') {

            this.getListCleranceNotImported();
        }
    }

    autocomplete = (time: number, callBack: Function) => (source$: Observable<any>) =>
        source$.pipe(
            debounceTime(time),
            distinctUntilChanged(),
            tap(() => {
                this.isLoading = true;
            }),
            switchMap((...args: any[]) => callBack(...args).pipe(
                takeUntil(source$.pipe(skip(1)))
            )
            )
        )

    selectCustom(custom: any) {
        this.isShow = false;
        this.customNo.setValue(custom.id);
        this.selectedCustom = custom;
    }

    clearMultipleSearch() {
        this.customNoSearch = '';
        this.getListCleranceNotImported();


    }
    closepp(param: string) {
        this.customNoSearch = param;
        this.getListCleranceNotImported();
    }


    async updateShipmentVolumn(importedData) {
        if (importedData != null) {
            this.currentJob.sumGrossWeight = 0;
            this.currentJob.sumNetWeight = 0;
            this.currentJob.sumCbm = 0;
            if (importedData.length > 0) {
                for (let i = 0; i < importedData.length; i++) {
                    this.currentJob.sumGrossWeight = this.currentJob.sumGrossWeight + importedData[i].grossWeight == null ? 0 : importedData[i].grossWeight;
                    this.currentJob.sumNetWeight = this.currentJob.sumNetWeight + importedData[i].netWeight == null ? 0 : importedData[i].netWeight;
                    this.currentJob.sumCbm = this.currentJob.sumCbm + importedData[i].cbm == null ? 0 : importedData[i].cbm;
                }
            }
            if (this.currentJob.sumGrossWeight === 0) {
                this.currentJob.sumGrossWeight = null;
            }
            if (this.currentJob.sumNetWeight === 0) {
                this.currentJob.sumNetWeight = null;
            }
            if (this.currentJob.sumCbm === 0) {
                this.currentJob.sumCbm = null;
            }
            await this.baseServices.putAsync(this.api_menu.Documentation.Operation.update, this.currentJob, false, false);
        }
    }
    getListCleranceNotImported() {
        this.notImportedCustomClearances = [];
        this.isLoading = true;
        if (this.customNo.value != '') {
            if (this.customNoSearch != 'isMultiple' && this.customNoSearch != '') {
                if (this.customNo.value != '') {
                    this.customNo.setValue('');
                }
                this.strKeySearch = this.customNo.value + ',' + this.customNoSearch;
                this.strKeySearch = this.customNoSearch;
            }
            else {
                this.strKeySearch = this.customNo.value;
            }
        } else {
            this.strKeySearch = this.customNoSearch;
        }
        this.strKeySearch = this.strKeySearch.trim().replace(/(?:\r\n|\r|\n|\\n|\\r|\\t)/g, ',').trim().split(',').filter(i => Boolean(i)).toString();
        this._operationRepo.getListNotImportToJob(this.strKeySearch, this.currentJob.customerId, false, this.pager.currentPage, this.pager.pageSize)
            .pipe(
                finalize(() => { this.isLoading = false; })
            )
            .subscribe(
                (res: any) => {
                    this.notImportedCustomClearances = res.data || [];
                    this.pager.totalItems = res.totalItems;


                },
                (error: any) => {
                },
                () => { }
            );
    }

    async showPopupSearch() {
        this.popupSearchMultiple.show();
        this.customNo.setValue('');
    }
    // app list
    setSortBy(sort?: string, order?: boolean): void {
        this.sort = sort ? sort : 'code';
        this.order = order;
    }

    sortBy(sort: string): void {
        if (!!sort) {
            this.setSortBy(sort, this.sort !== sort ? true : !this.order);

            if (typeof (this.requestSort) === 'function') {
                this.requestSort(this.sort, this.order);   // sort Local
            }
        }
    }

    sortClass(sort: string): string {
        if (!!sort) {
            let classes = 'sortable ';
            if (this.sort === sort) {
                classes += ('sort-' + (this.order ? 'asc' : 'desc') + ' ');
            }

            return classes;
        }
        return '';
    }

    close() {
        this.customNoSearch = '';
        this.hide();
    }
}
