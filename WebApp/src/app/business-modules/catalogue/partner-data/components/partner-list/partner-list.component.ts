import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { catchError, finalize } from 'rxjs/operators';
import { SortService } from '@services';
import { Partner } from '@models';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';


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
    headerSalemans: CommonInterface.IHeaderTable[];
    isCustomer = false;
    saleMans: any[] = [];
    services: any[] = [];

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
        this.getService();
        this.headerSalemans = [
            { title: 'No', field: '', sortable: true },
            { title: 'Service', field: 'service', sortable: true },
            { title: 'Office', field: 'office', sortable: true },
            { title: 'Company', field: 'company', sortable: true },
            { title: 'Salesman', field: 'username', sortable: true },
            { title: 'Status', field: 'status', sortable: true },
            { title: 'CreateDate', field: 'createDate', sortable: true }
        ];
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
        if (this.criteria.partnerGroup === PartnerGroupEnum.CUSTOMER) {
            this.isCustomer = true;
        } else {
            this.isCustomer = false;
        }
        console.log(this.dataSearch);
        this.getPartners();
    }
    replaceService() {
        for (const item of this.saleMans) {
            this.services.forEach(itemservice => {
                if (item.service === itemservice.id) {
                    item.service = itemservice.text;
                }
            });
        }
    }
    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                    }
                },
            );
    }
    showSaleman(partnerId: string) {
        this._catalogueRepo.getListSaleman(partnerId)
            .pipe(catchError(this.catchError), finalize(() => {
            })).subscribe(
                (res: any) => {
                    if (res !== null) {
                        this.saleMans = res;
                        console.log(this.saleMans);
                        this.replaceService();
                    } else {
                        this.saleMans = [];
                    }

                }
            );
        // if (!!this.customers[indexs].saleManRequests.length) {
        //     this.saleMans = this.customers[indexs].saleManRequests;
        //     this.replaceService();
        // } else {
        //     this._progressRef.start();
        //     this._catalogueRepo.getListSaleman(partnerId)
        //         .pipe(
        //             catchError(this.catchError),
        //             finalize(() => this._progressRef.complete())
        //         ).subscribe(
        //             (res: Saleman[]) => {
        //                 this.saleMans = res || [];
        //                 this.customers[indexs].saleManRequests = this.saleMans;
        //                 this.replaceService();
        //             }
        //         );
        // }
    }
    sortBySaleman(sortData: CommonInterface.ISortData): void {
        if (!!sortData.sortField) {
            this.saleMans = this._sortService.sort(this.saleMans, sortData.sortField, sortData.order);
        }
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
