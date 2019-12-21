import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';
import { Partner } from '@models';


@Component({
    selector: 'app-partner-list',
    templateUrl: './partner-list.component.html'
})
export class PartnerListComponent extends AppList implements OnInit {
    // @Input() type = 0;
    @Input() criteria: any = {};
    @Output() deleteConfirm = new EventEmitter<Partner>();
    @Output() detail = new EventEmitter<Partner>();
    partners: any[] = [];

    constructor(private _ngProgressService: NgProgress,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService) {
        super();

        this._progressRef = this._ngProgressService.ref();
        this.requestSearch = this.searchPartner;
        this.requestList = this.getPartners;
        this.requestSort = this.sortPartners;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Partner ID', field: 'id', sortable: true },
            { title: 'Name ABBR', field: 'shortName', sortable: true },
            { title: 'Billing Address', field: 'addressVn', sortable: true },
            { title: 'Tax Code', field: 'taxCode', sortable: true },
            { title: 'Tel Address', field: 'tel', sortable: true },
            { title: 'Fax', field: 'fax', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Modify', field: 'datetimeModified', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];
        this.dataSearch = this.criteria;
        console.log(this.dataSearch);
        this.getPartners();
    }

    searchPartner(event: CommonInterface.ISearchOption) {
        this.dataSearch = {};
        this.dataSearch[event.field] = event.searchString;
    }

    getPartners() {
        this.isLoading = true;
        this._progressRef.start();
        this._catalogueRepo.getListPartner(this.page, this.pageSize, this.dataSearch)
            .pipe(catchError(this.catchError), finalize(() => {
                this._progressRef.complete();
                this.isLoading = false;
            })).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.partners = res.data || [];
                    console.log(this.partners);
                    this.totalItems = res.totalItems;
                }
            );
    }

    sortPartners() {
        this.partners = this._sortService.sort(this.partners, this.sort, this.order);
    }
    showDetail(item) {
        this.detail.emit(item);
    }
    showConfirmDelete(item) {
        this.deleteConfirm.emit(item);
    }
}
