import { Component, Input, ViewChild } from '@angular/core';
import { ConfirmPopupComponent } from '@common';
import { SystemConstants } from '@constants';
import { AddressPartner, Partner } from '@models';
import { NgProgress } from '@ngx-progressbar/core';
import { CatalogueRepo } from '@repositories';
import { SortService } from '@services';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';
import { AppList } from 'src/app/app.list';
import { FormAddressCommercialCatalogueComponent } from 'src/app/business-modules/share-modules/components/form-address-commercial-catalogue/form-address-commercial-catalogue.component';
@Component({
    selector: 'app-commercial-address-list',
    templateUrl: './commercial-address-list.component.html',
})
export class CommercialAddressListComponent extends AppList {
    @ViewChild(ConfirmPopupComponent) confirmDeletePopup: ConfirmPopupComponent;
    @ViewChild(FormAddressCommercialCatalogueComponent) formUpdateAddressPopup: FormAddressCommercialCatalogueComponent;
    //@Input() partnerId: string;
    addresses: AddressPartner[] = [];
    partnerId: string = '';
    partner: Partner;
    isUpdate: Boolean = false;
    id: string = '';
    indexLstAddress: number = null;

    constructor(
        private _ngProgressService: NgProgress,
        private _sortService: SortService,
        private _catalogueRepo: CatalogueRepo,
        private _toastService: ToastrService,
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
    }
    ngOnInit() {
        this.headers = [
            { title: 'Name Address ABBR', field: 'shortNameAddress', sortable: true },
            { title: 'Address', field: 'location', sortable: true },
            { title: 'Address Type', field: 'addressType', sortable: true },
            { title: 'Contract Person', field: 'contactPerson', sortable: false },
            { title: 'Phone Number', field: 'tel', sortable: false },

        ];
        this.getDataCombobox();
    }
    getDataCombobox() {
        forkJoin([
            this._catalogueRepo.getCountryByLanguage(),
            this._catalogueRepo.getProvinces(),
            this._catalogueRepo.getDistricts(),
            this._catalogueRepo.getWards()
        ])
            .pipe(catchError(this.catchError))
            .subscribe(
                ([countries, provinces, districts, wards]) => {
                    this.formUpdateAddressPopup.countries = this.utility.prepareNg2SelectData(countries || [], 'id', 'name');
                    this.formUpdateAddressPopup.provinces = this.utility.prepareNg2SelectData(provinces || [], 'id', 'nameVn');
                    this.formUpdateAddressPopup.districts = this.utility.prepareNg2SelectData(districts || [], 'id', 'nameVn');
                    this.formUpdateAddressPopup.wards = this.utility.prepareNg2SelectData(wards || [], 'id', 'nameVn');
                },
                () => { },
            );
    }
    getAddressPartner(partnerId: string) {
        this.isLoading = true;
        this._catalogueRepo.getAddressPartner(partnerId)
            .pipe(catchError(this.catchError), finalize(() => {
                this.isLoading = false;
            })).subscribe(
                (res: AddressPartner[]) => {
                    this.addresses = res || [];
                }
            );
    }
    showConfirmDelete(id: string, index: number) {
        this.id = id;
        if (this.id === SystemConstants.EMPTY_GUID) {
            this.addresses = [...this.addresses.slice(0, index), ...this.addresses.slice(index + 1)];
        } else {
            this.confirmDeletePopup.show();
        }
    }

    showDetail(address: any, index: number = null) {
        this.id = address.id;
        this.formUpdateAddressPopup.isUpdate = true;
        this.formUpdateAddressPopup.id = this.id;
        this.formUpdateAddressPopup.partnerId = this.partnerId;
        this.formUpdateAddressPopup.indexDetailAddress = index;
        this.formUpdateAddressPopup.getProvinces(address.countryId);
        this.formUpdateAddressPopup.getDistricts(address.cityId);
        this.formUpdateAddressPopup.getWards(address.districtId);
        if (!!this.formUpdateAddressPopup.partnerId) {
            this._catalogueRepo.getDetailAddress(address.id)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: AddressPartner) => {
                        if (!!res) {
                            this.formUpdateAddressPopup.selectedAddress = res;
                            this.formUpdateAddressPopup.setFormValue(res);
                            this.formUpdateAddressPopup.show();
                        }
                    }
                );
        }
        else {
            this.formUpdateAddressPopup.selectedAddress = this.addresses[this.indexLstAddress];
            // this.formUpdateAddressPopup.updateFormValue(data);
            this.formUpdateAddressPopup.show();
        }

    }

    onDelete() {
        this.confirmDeletePopup.hide();
        this._catalogueRepo.deleteAddress(this.id)
            .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
            .subscribe(
                (res: CommonInterface.IResult) => {
                    if (res.status) {
                        this._toastService.success(res.message);
                        this.getAddressPartner(this.partnerId);
                    } else {
                        this._toastService.error(res.message);
                    }
                }
            );
    }

    onRequestAddress($event: any) {
        const data = $event;
        if (data === true) {
            this.formUpdateAddressPopup.hide();
            this.getAddressPartner(this.partnerId);
        }
    }

    showPopupAddNew() {
        this.formUpdateAddressPopup.partnerId = this.partnerId;
        this.formUpdateAddressPopup.partner = this.partner;
        this.formUpdateAddressPopup.isUpdate = false;
        this.formUpdateAddressPopup.isSubmitted = false;
        if (!this.formUpdateAddressPopup.isUpdate) {
            this.formUpdateAddressPopup.formAddress.reset();
            this.formUpdateAddressPopup.formAddress.setErrors(null);
            this.formUpdateAddressPopup.setDefaultValue(this.partner);
            
        }
        this.formUpdateAddressPopup.show();
    }
}