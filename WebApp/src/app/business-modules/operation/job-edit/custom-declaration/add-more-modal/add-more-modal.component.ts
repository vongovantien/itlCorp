import { Component, OnInit, Input, Output, EventEmitter, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SortService } from 'src/app/shared/services';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { takeUntil, debounceTime, switchMap, skip, distinctUntilChanged, finalize, tap } from 'rxjs/operators';
import { BehaviorSubject, Observable } from 'rxjs';
import { PAGINGSETTING } from 'src/constants/paging.const';
import { PagerSetting } from 'src/app/shared/models/layout/pager-setting.model';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';

import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';
import { SearchMultipleComponent } from '../components/search-multiple/search-multiple.component';
import { OperationRepo, DocumentationRepo } from 'src/app/shared/repositories';

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
    sort: string = null;
    order: any = false;
    keyword: string = '';
    requestList: any = null;
    requestSort: any = null;
    notImportedCustomClearances: any[] = [];
    customClearances: any[] = [];
    checkAllNotImported = false;
    headers: CommonInterface.IHeaderTable[];
    form: FormGroup;
    customNo: AbstractControl;
    selectedCustom: any = null;
    // keyMultipleSearch: string = '';
    strKeySearch: string = '';

    searchCustomButtonSetting: ButtonModalSetting = {
        dataTarget: "search-custom-modal",
        typeButton: ButtonType.add
    };
    constructor(
        private _fb: FormBuilder,
        private _sortService: SortService,
        private _documentationRepo: DocumentationRepo,
        private _operationRepo: OperationRepo) {
        super();
        this.requestSort = this.sortLocal;
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

        this.requestList = this.getListCleranceNotImported;
        // this.term$.pipe(
        //     tap(
        //         () => {
        //             this.isLoading = true;
        //         }
        //     ),

        //     distinctUntilChanged(),
        //     finalize(() => {
        //         this.isLoading = false;
        //     }),
        //     this.autocomplete(300, ((term: any) => this._operationRepo.getListNotImportToJob(this.customNo.value, this.currentJob.customerId, false, this.page, this.pageSize)))
        // ).subscribe(
        //     (res: any) => {

        //         this.notImportedCustomClearances = res.data || [];
        //         // this.pager.totalItems = res.totalItems;
        //         // this.pager.totalPages = res.totalPages;
        //     },
        //     (error: any) => { },
        //     () => { }
        // );
    }

    sortLocal(sort: string): void {
        this.notImportedCustomClearances = this._sortService.sort(this.notImportedCustomClearances, sort, this.order);
    }

    refreshData() {
        this.keyword = '';
        this.page = 1;
        this.pageSize = this.numberToShow[1];
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
        // const checkedData = this.notImportedCustomClearances.filter(x => x.isChecked === true);
        // if (checkedData.length > 0) {
        //     for (let i = 0; i < checkedData.length; i++) {
        //         const index = this.notImportedData.indexOf(x => x.id === checkedData[i].id);
        //         if (index > -1) {
        //             this.notImportedData[index] = true;
        //         }
        //     }
        // }
    }


    initForm() {
        this.form = this._fb.group({
            customNo: ['', Validators.compose([
                Validators.required
            ])]
        });
        this.customNo = this.form.controls['customNo'];
    }

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

    updateJobToClearance() {
        const dataToUpdate = this.notImportedCustomClearances.filter(x => x.isChecked === true);
        if (dataToUpdate.length > 0) {
            dataToUpdate.forEach(x => {
                x.jobNo = this.currentJob.jobNo;
            });

            this._operationRepo.updateJobToClearances(dataToUpdate)
                .pipe(finalize(() => this.hide()))
                .subscribe(
                    (responses: CommonInterface.IResult | any) => {
                        if (responses.success === true) {
                            this.updateShipmentVolumn(dataToUpdate);
                            this.isCloseModal.emit(true);
                        }
                    }
                );
        }
    }
    onSearchAutoComplete(keyword: string) {
        this.term$.next(keyword.trim());

        if (keyword !== null && keyword.length < 2 && keyword.length > 0) {
            return 0;
        }
        this.getListCleranceNotImported();
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

    // selectCustom(custom: any) {
    //     this.isShow = false;
    //     this.customNo.setValue(custom.id);
    //     this.selectedCustom = custom;
    // }
    closepp(param: string) {
        this.page = 1;
        this.pageSize = this.numberToShow[1];
        // this.keyMultipleSearch = param;
        // this.strKeySearch = param;
        console.log(this.popupSearchMultiple.customNoSearch);
        this.getListCleranceNotImported();
    }
    updateShipmentVolumn(importedData) {
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

            this._documentationRepo.updateShipment(this.currentJob).subscribe(
                (res) => {

                }
            );
        }
    }
    getListCleranceNotImported() {
        this.notImportedCustomClearances = [];
        // this.isLoading = true;
        if (this.customNo.value !== '') {
            if (this.popupSearchMultiple.customNoSearch !== '') {
                // if (this.customNo.value !== '') {
                //     this.customNo.setValue('');
                // }
                this.strKeySearch = this.customNo.value + ',' + this.popupSearchMultiple.customNoSearch;
                // this.strKeySearch = this.keyMultipleSearch;
            } else {
                this.strKeySearch = this.customNo.value;
            }
        } else {
            this.strKeySearch = this.popupSearchMultiple.customNoSearch;
        }
        // if (this.keyMultipleSearch !== '') {
        //     this.strKeySearch = this.keyMultipleSearch;
        // } else{

        // }
        // this.strKeySearch = this.strKeySearch.trim().replace(/(?:\r\n|\r|\n|\\n|\\r|\\t)/g, ',').trim().split(',').filter(i => Boolean(i)).toString();
        this.getClearanceNotImported();
    }
    getClearanceNotImported() {
        this._operationRepo.getListNotImportToJob(this.strKeySearch, this.currentJob.customerId, false, this.page, this.pageSize)
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
                },
                (error: any) => {
                },
                () => { }
            );
    }

    showPopupSearch() {
        // this.popupSearchMultiple.customNoSearch = '';
        // this.keyMultipleSearch = '';
        // this.customNo.setValue('');
        this.popupSearchMultiple.show();
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
        this.page = 1;
        this.pageSize = this.numberToShow[1];
        this.popupSearchMultiple.customNoSearch = '';
        this.strKeySearch = '';
        this.customNo.setValue('');
        this.hide();
    }
}
