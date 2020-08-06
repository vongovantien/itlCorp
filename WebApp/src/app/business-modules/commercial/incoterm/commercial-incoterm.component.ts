import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { Incoterm, IncotermModel } from '@models';
import { CatalogueRepo } from '@repositories';
import { catchError, finalize, map, tap, switchMap } from 'rxjs/operators';
import { CommercialFormSearchIncotermComponent } from './components/form-search-incoterm/form-search-incoterm.component';
import { SortService } from '@services';
import { SystemConstants } from '@constants';
@Component({
    selector: 'app-commercial-incoterm',
    templateUrl: './commercial-incoterm.component.html',
})
export class CommercialIncotermComponent extends AppList implements OnInit {

    @ViewChild("formSearch", { static: false }) formSearch: CommercialFormSearchIncotermComponent;

    incoterms: IncotermModel[];

    criteria: any = {};

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
    ) {
        super();
        this.requestList = this.getIncotermListPaging;
        this.requestSort = this.sortIncotermList;
    }

    ngOnInit(): void {
        this.headers = [
            { title: 'Incoterm', field: 'code', sortable: true },
            { title: 'Name En', field: 'nameEn', sortable: true },
            { title: 'Sevice', field: 'service', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
            { title: 'Create Date', field: 'datetimeCreated', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },

        ];
        this.getIncotermListPaging();
    }

    getIncotermListPaging() {
        this._catalogueRepo.getIncotermListPaging(this.page, this.pageSize, Object.assign({}, this.criteria))
            .pipe(
                catchError(this.catchError),
                // finalize(() => { this.isLoading = false; this._progressRef.complete(); }),
                map((res: any) => {
                    return {
                        data: res.data,
                        totalItems: res.totalItems,
                    };
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    console.log(res);

                    this.totalItems = res.totalItems || 0;
                    this.incoterms = (res.data || []).map(i => new IncotermModel(i)) || [];
                    console.log(this.incoterms);

                },
            );
    }

    onSearchIncoterm(event) {
        this.criteria = event;
        console.log(this.criteria);
        this.page = 1;
        this.getIncotermListPaging();
    }

    onResetIncoterm(event) {
        this.criteria = event;
        this.formSearch.initForm();
        this.getIncotermListPaging();
    }

    sortIncotermList(sortField: string, order: boolean) {
        this.incoterms = this._sortService.sort(this.incoterms, sortField, order);
    }

    exportExcel() {
        console.log("criteria in export: ", this.criteria);
        this._catalogueRepo.downloadIncotermListExcel(this.criteria)
            .subscribe(
                (res: Blob) => {
                    this.downLoadFile(res, SystemConstants.FILE_EXCEL, 'incoterm-list.xlsx');
                }
            );
    }
}

