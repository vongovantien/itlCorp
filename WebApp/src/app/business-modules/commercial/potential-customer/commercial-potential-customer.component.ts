import { Component, OnInit, ViewChild } from '@angular/core';
import { AppList, IPermissionBase } from 'src/app/app.list';
import { POTENTIALCUSTOMERCOLUMNSETTING } from './commercial-potential-customer.component.columns';
import { ColumnSetting } from 'src/app/shared/models/layout/column-setting.model';
import { TypeSearch } from 'src/app/shared/enums/type-search.enum';
import { CatPotentialModel, PotentialUpdateModel } from '@models';
import { CatalogueRepo } from '@repositories';
import { catchError, finalize, map, switchMap } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { SortService } from '@services';
import { CommercialPotentialCustomerPopupComponent } from './components/popup/potential-customer-commercial.popup';
import { of } from 'rxjs';

@Component({
    selector: 'app-commercial-potential-customer',
    templateUrl: './commercial-potential-customer.component.html',
})
export class CommercialPotentialCustomerComponent extends AppList implements OnInit, IPermissionBase {
    @ViewChild(CommercialPotentialCustomerPopupComponent, { static: false }) potentialCustomerPopup: CommercialPotentialCustomerPopupComponent;
    //
    searchPotentialSettings: ColumnSetting[] = POTENTIALCUSTOMERCOLUMNSETTING;
    configSearch: any = {
        settingFields: this.searchPotentialSettings.filter(x => x.allowSearch === true).map(x => ({ "fieldName": x.primaryKey, "displayName": x.header })),
        typeSearch: TypeSearch.intab
    };
    //
    potentialCustomers: CatPotentialModel[] = [];

    constructor(
        private _ngProgressService: NgProgress,
        private _sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
        this.requestList = this.getPotentialCustomerListPaging;
        this.requestSort = this.sortPotentialCustomerList;
        this._progressRef = this._ngProgressService.ref();
    }
    ngOnInit(): void {
        this.headers = [
            { title: 'English Name', field: 'nameEn', sortable: true },
            { title: 'Local Name', field: 'nameLocal', sortable: true },
            { title: 'Tax Code', field: 'taxcode', sortable: true },
            { title: 'Tel', field: 'tel', sortable: true },
            { title: 'Address', field: 'address', sortable: true },
            { title: 'Email', field: 'email', sortable: true },
            { title: 'Margin', field: 'margin', sortable: true },
            { title: 'Quotation', field: 'quotation', sortable: true },
            { title: 'Creator', field: 'userCreatedName', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];
        this.getPotentialCustomerListPaging();
    }
    //
    getPotentialCustomerListPaging() {
        this.isLoading = true;
        this._catalogueRepo.getPotentialCustomerListPaging(this.page, this.pageSize, Object.assign({}, this.dataSearch))
            .pipe(
                catchError(this.catchError),
                finalize(() => { this.isLoading = false; }),
                map((res: any) => {
                    return {
                        data: res.data,
                        totalItems: res.totalItems,
                    };
                })
            ).subscribe(
                (res: CommonInterface.IResponsePaging) => {
                    this.totalItems = res.totalItems || 0;
                    this.potentialCustomers = (res.data || []).map(i => new CatPotentialModel(i)) || [];

                },
            );
    }
    //
    sortPotentialCustomerList(sortField: string, order: boolean) {
        this.potentialCustomers = this._sortService.sort(this.potentialCustomers, sortField, order);
    }
    //
    checkAllowDetail(potentialId: string): void {
        this._catalogueRepo.checkAllowGetDetailPotential(potentialId)
            .pipe(
                switchMap(
                    (res: boolean) => {
                        if (res) {
                            this._progressRef.start();
                            return this._catalogueRepo.getDetailPotential(potentialId);
                        } else {
                            return of(false);
                        }
                    }
                ),
            )
            .subscribe(
                (res: PotentialUpdateModel | boolean) => {
                    if (typeof res === "boolean") {

                    }
                    else {

                    }
                }
            );
        [this.potentialCustomerPopup.isUpdate, this.potentialCustomerPopup.isSubmitted] = [true, false];
        this.potentialCustomerPopup.show();
    }
    //
    checkAllowDelete(T: any): void {
        throw new Error("Method not implemented.");
    }
    //
    onSearchPotentialCustomer(event) {
        this.dataSearch = { [event.field]: event.searchString };

        this.page = 1;
        this.getPotentialCustomerListPaging();
    }
    //
    onResetPotentialCustomer(event) {
        this.onSearchPotentialCustomer(event);
    }
    //
    addNew() {
        [this.potentialCustomerPopup.isUpdate, this.potentialCustomerPopup.isSubmitted] = [false, false];
        this.potentialCustomerPopup.initForm();
        this.potentialCustomerPopup.show();
    }
    //
    handleChangePotentialPopup(event: any) {
        this.dataSearch = {};
        this.getPotentialCustomerListPaging();
    }

}

