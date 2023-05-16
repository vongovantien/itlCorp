import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';

import { AppList } from 'src/app/app.list';

import { SortService } from '@services';
import { CatalogueRepo } from '@repositories';
import { ConfirmPopupComponent, SearchOptionsComponent } from '@common';
import { CommonEnum } from '@enums';
import * as fromLocationStore from './../store';
import { FormCountryPopupComponent } from './add-country/add-country.component';
import { CountryModel } from 'src/app/shared/models/catalogue/country.model';

import { catchError, finalize, map, takeUntil } from 'rxjs/operators';
import { Store } from '@ngrx/store';
import { getCountryDataSearch, getLocationCountryLoadingState, getLocationCountryState, ICatLocationState, LoadListCountryLocation, SearchListCountry } from './../store';



@Component({
    selector: 'app-country',
    templateUrl: './country.component.html'
})

export class AppCountryComponent extends AppList implements OnInit {

    @ViewChild(FormCountryPopupComponent) formCountry: FormCountryPopupComponent;
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(SearchOptionsComponent, { static: true }) searchOptionsComponent: SearchOptionsComponent;


    countries: CountryModel[] = [];
    selectedCountry: CountryModel;

    configSearch: CommonInterface.IConfigSearchOption = {
        settingFields: [
            { fieldName: 'code', displayName: 'Code' },
            { fieldName: 'nameEn', displayName: 'English Name' },
            { fieldName: 'nameVn', displayName: 'Local Name' }
        ],
        typeSearch: CommonEnum.TypeSearch.outtab
    };

    constructor(
        private _ngProgressService: NgProgress,
        private _catalogueRepo: CatalogueRepo,
        private _sortService: SortService,
        private _toastService: ToastrService,
        private _store: Store<ICatLocationState>,
        private _cd: ChangeDetectorRef

    ) {
        super();

        this._progressRef = this._ngProgressService.ref();
        this.requestSearch = this.searchCountry;
        this.requestList = this.requestListCountry;
        this.requestSort = this.sortCountries;
    }

    ngOnInit() {
        this.headers = [
            { title: 'Code', field: 'code', sortable: true },
            { title: 'Name EN', field: 'nameEn', sortable: true },
            { title: 'Name Local', field: 'nameVn', sortable: true },
            { title: 'Status', field: 'active', sortable: true },
        ];
        this._store.select(getCountryDataSearch)
        .pipe(
            catchError(this.catchError),
            takeUntil(this.ngUnsubscribe),
            map((dataSearch) => ({ dataSearch: dataSearch }))
        ).subscribe(
            (res: any) => {
                if (!!res && !!Object.keys(res?.dataSearch).length) {
                    this.dataSearch = res.dataSearch;
                }
            },
        );
        this.getCountries();
        this.requestListCountry();
    }
    ngAfterViewInit() {
        if (Object.keys(this.dataSearch).length > 0) {
            this.searchOptionsComponent.searchObject.searchString = this.dataSearch.keyword;
            this._cd.detectChanges();
        }
    }
    searchCountry(event: CommonInterface.ISearchOption) {
        this.dataSearch = {};
        this.dataSearch[event.field] = event.searchString;
        this.dataSearch.keyword = event.searchString;
        this._store.dispatch(SearchListCountry({ payload: this.dataSearch }));
        this.requestListCountry();
        // this.getCountries();
    }
    requestListCountry() {
        this._store.dispatch(LoadListCountryLocation({ page: this.page, size: this.pageSize, dataSearch: this.dataSearch }));
    }
    getCountries() {
        // this.isLoading = true;
        // this._progressRef.start();
        // this._catalogueRepo.pagingCountry(this.page, this.pageSize, this.dataSearch)
        //     .pipe(catchError(this.catchError), finalize(() => {
        //         this._progressRef.complete();
        //         this.isLoading = false;
        //     })).subscribe(
        //         (res: CommonInterface.IResponsePaging) => {
        //             this.countries = res.data || [];
        //             this.totalItems = res.totalItems;
        //         }
        //     );
        this._store.select(getLocationCountryState)
        .pipe(
            catchError(this.catchError),
            takeUntil(this.ngUnsubscribe),
            map((data: CommonInterface.IResponsePaging | any) => {
                return {
                    data: data.data,
                    totalItems: data.totalItems,
                };
            })
        ).subscribe(
            (res: any) => {
                this.countries = res.data || [];
                this.totalItems = res.totalItems || 0;
            }
        );
    }

    sortCountries() {
        this.countries = this._sortService.sort(this.countries, this.sort, this.order);
    }

    resetSearch() {
        this.dataSearch = {};
        this.requestListCountry()
        this._store.dispatch(SearchListCountry({ payload: {} }));
        this.getCountries();
    }

    showAdd() {
        this.formCountry.isSubmitted = false;
        this.formCountry.formAddCountry.reset();
        this.formCountry.show();
    }

    showDetail(country: CountryModel) {
        this.selectedCountry = country;
        if (this.selectedCountry.id) {
            this.formCountry.isUpdate = true;
            this.formCountry.isSubmitted = false;
            this.formCountry.id = country.id;

            this.formCountry.formAddCountry.patchValue(this.selectedCountry);
            this.formCountry.show();
        }
    }

    showConfirmDelete(country: CountryModel) {
        this.selectedCountry = country;
        this.confirmDeletePopup.show();
    }

    onDelete() {
        this.confirmDeletePopup.hide();

        this._progressRef.start();
        this._catalogueRepo.deleteCountry(this.selectedCountry.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);

                        this.resetSearch();
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }
}

