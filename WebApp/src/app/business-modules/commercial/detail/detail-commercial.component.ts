import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';

import { CommercialCreateComponent } from '../create/create-commercial.component';

import { finalize, catchError, concatMap, map } from 'rxjs/operators';
import { of, combineLatest } from 'rxjs';
import { ConfirmPopupComponent } from '@common';
import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';


@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})
export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {
    @ViewChild(CommercialFormCreateComponent, { static: false }) formCommercialComponent: CommercialFormCreateComponent;
    @ViewChild('internalReferenceConfirmPopup', { static: false }) confirmTaxcode: ConfirmPopupComponent;
    partnerId: string;
    partner: Partner;

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        private _activedRoute: ActivatedRoute,
        private _cd: ChangeDetectorRef,
        protected _ngProgressService: NgProgress,
    ) {
        super(_router, _toastService, _catalogueRepo, _ngProgressService, _activedRoute);
    }

    ngOnInit(): void { }

    ngAfterViewInit() {

        combineLatest([
            this._activedRoute.params,
            this._activedRoute.data,
        ]).pipe(
            map(([p, d]) => ({ ...p, ...d }))
        ).subscribe(
            (res: any) => {
                if (res.type) {
                    this.type = res.type;
                }
                if (res.partnerId) {
                    this.partnerId = res.partnerId;
                    this.contractList.partnerId = this.partnerId;

                    this.getDetailCustomer(this.partnerId);
                    this.contractList.getListContract(this.partnerId);
                } else {
                    this.gotoList();
                }
            });
        this._cd.detectChanges();

    }

    onFocusInternalReference() {
        this.confirmTaxcode.hide();
        //
        this.formCommercialComponent.handleFocusInternalReference();
    }

    getDetailCustomer(partnerId: string) {
        this._catalogueRepo.getDetailPartner(partnerId)
            .subscribe(
                (res: Partner) => {
                    this.partner = res;
                    console.log("detail partner:", this.partner);
                    // this.formCreate.formGroup.patchValue(res);
                    this.setDataForm(this.partner);
                    // this.formCreate.partnerLocation.setValue([<CommonInterface.INg2Select>{ id: this.partner.partnerLocation, text: this.partner.partnerLocation }]);

                    this.formCreate.getShippingProvinces(res.countryShippingId);
                    this.formCreate.getBillingProvinces(res.countryId);
                }
            );
    }

    setDataForm(partner: Partner) {
        this.formCreate.formGroup.patchValue({
            accountNo: partner.accountNo,
            partnerNameEn: partner.partnerNameEn,
            partnerNameVn: partner.partnerNameVn,
            shortName: partner.shortName,
            taxCode: partner.taxCode,
            internalReferenceNo: partner.internalReferenceNo,
            addressShippingEn: partner.addressShippingEn,
            addressShippingVn: partner.addressShippingVn,
            addressVn: partner.addressVn,
            addressEn: partner.addressEn,
            zipCode: partner.zipCode,
            zipCodeShipping: partner.zipCodeShipping,
            contactPerson: partner.contactPerson,
            tel: partner.tel,
            fax: partner.fax,
            workPhoneEx: partner.workPhoneEx,
            email: partner.email,
            billingEmail: partner.billingEmail,
            billingPhone: partner.billingPhone,
            countryId: partner.countryId,
            countryShippingId: partner.countryShippingId,
            provinceId: partner.provinceId,
            provinceShippingId: partner.provinceShippingId,
            partnerLocation: !!partner.partnerLocation ? [<CommonInterface.INg2Select>{ id: partner.partnerLocation, text: partner.partnerLocation }] : null,
            parentId: partner.parentId
        });
    }

    getListContract(partneId: string) {
        this._cd.detectChanges();

        // this.contractList.isLoading = true;
        this._catalogueRepo.getListSaleManDetail({ partnerId: partneId })
            .pipe(
                finalize(() => this.contractList.isLoading = false)
            )
            .subscribe(
                (res: any[]) => {
                    this.contractList.contracts = res || [];
                    console.log(this.contractList);
                }
            );
    }

    onSave() {
        this.formCreate.isSubmitted = true;

        if (!this.formCreate.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        if (!this.contractList.contracts.length) {
            this._toastService.warning("Partner don't have any contract in this period, Please check it again!");
            return;
        }

        const modelAdd: Partner = this.formCreate.formGroup.getRawValue();
        modelAdd.contracts = this.contractList.contracts;

        modelAdd.id = this.partnerId;
        modelAdd.userCreated = this.partner.userCreated;
        modelAdd.datetimeCreated = this.partner.datetimeCreated;
        modelAdd.partnerType = this.partner.partnerType;
        modelAdd.partnerGroup = this.partner.partnerGroup;
        modelAdd.datetimeCreated = this.partner.datetimeCreated;

        // * Update catalogue partner data.
        modelAdd.roundUpMethod = this.partner.roundUpMethod;
        modelAdd.applyDim = this.partner.applyDim;
        modelAdd.active = this.partner.active;
        modelAdd.applyDim = this.partner.applyDim;
        modelAdd.coLoaderCode = this.partner.coLoaderCode;
        modelAdd.swiftCode = this.partner.swiftCode;
        modelAdd.coLoaderCode = this.partner.coLoaderCode;
        modelAdd.website = this.partner.website;
        modelAdd.bankAccountNo = this.partner.bankAccountNo;
        modelAdd.bankAccountName = this.partner.bankAccountName;
        modelAdd.bankAccountAddress = this.partner.bankAccountAddress;
        modelAdd.note = this.partner.note;
        modelAdd.public = this.partner.public;
        modelAdd.workPlaceId = this.partner.workPlaceId;
        modelAdd.partnerMode = this.partner.partnerMode;
        modelAdd.partnerLocation = !!this.formCreate.partnerLocation.value[0].id ? this.formCreate.partnerLocation.value[0].id : this.formCreate.partnerLocation.value;

        console.log(modelAdd);
        this.updatePartner(modelAdd);
    }

    updateStatusPartner($event) {
        if (this.partner.active === false) {
            this.partner.active = $event;
        }
    }

    updatePartner(body: Partner) {
        const bodyValidateTaxcode = {
            id: body.id,
            taxCode: body.taxCode,
            internalReferenceNo: body.internalReferenceNo
        };
        this._catalogueRepo.checkTaxCode(bodyValidateTaxcode)
            .pipe(
                map((value: Partner) => {
                    if (!!value) {
                        if (!!body.internalReferenceNo) {
                            this.invalidTaxCode = `This Parnter is existed, please you check again!`;
                            this.infoPopupTaxCode.show();
                        } else {
                            this.invalidTaxCode = `This <b>Taxcode</b> already <b>Existed</b> in  <b>${value.shortName}</b>, If you want to Create Internal account, Please fill info to <b>Internal Reference Info</b>.`;
                            this.confirmTaxcode.show();
                        }
                        throw new Error("TaxCode Duplicated: ");
                    }
                    return value;
                }),
                catchError((err, caught) => of(false)),
                concatMap((responseValidateTaxCode: boolean) => {
                    if (responseValidateTaxCode === false) {
                        return of(false);
                    }
                    return this._catalogueRepo.updatePartner(body.id, body)
                        .pipe(
                            catchError(this.catchError),
                            finalize(() => this._progressRef.complete()),

                            concatMap((res: CommonInterface.IResult) => {
                                if (res.status) {
                                    this._toastService.success(res.message);
                                    return this._catalogueRepo.getDetailPartner(this.partnerId);
                                }
                                return of(res);
                            }),
                        );
                })
            )
            .subscribe(
                (res: any) => {
                    console.log(res);
                    if (res === false) {
                        //this.infoPopupTaxCode.show();
                        this.formCreate.isExistedTaxcode = true;
                        return;
                    }
                    if (res || res.status) {
                        this.partner = res;
                        console.log("detail partner:", this.partner);
                        this.formCreate.formGroup.patchValue(res);
                    } else {
                        this._toastService.error(res.message);
                    }
                },
                (error: HttpErrorResponse) => {
                    console.log(error);
                }
            );
    }
}

