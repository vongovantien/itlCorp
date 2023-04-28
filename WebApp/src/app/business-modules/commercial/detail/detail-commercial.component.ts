import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { ToastrService } from 'ngx-toastr';
import { PayableComponent } from '../components/payable/payable.component';

import { Partner } from '@models';
import { CatalogueRepo, SystemFileManageRepo } from '@repositories';

import { CommercialCreateComponent } from '../create/create-commercial.component';

import { ConfirmPopupComponent } from '@common';
import { CommonEnum } from '@enums';
import { Store } from '@ngrx/store';
import { IAppState } from '@store';
import { combineLatest, of } from 'rxjs';
import { catchError, concatMap, finalize, map } from 'rxjs/operators';
import { CommercialBranchSubListComponent } from '../components/branch-sub/commercial-branch-sub-list.component';
import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialBankListComponent } from '../components/bank/commercial-bank-list.component';
import { formatDate } from '@angular/common';
import { CommercialAddressListComponent } from '../components/address/commercial-address-list.component';

@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})

export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {
    @ViewChild(CommercialFormCreateComponent) formCommercialComponent: CommercialFormCreateComponent;
    @ViewChild('internalReferenceConfirmPopup') confirmTaxcode: ConfirmPopupComponent;
    @ViewChild(CommercialBranchSubListComponent) formBranchSubList: CommercialBranchSubListComponent;
    @ViewChild(CommercialBankListComponent) formBankList: CommercialBankListComponent
    @ViewChild(PayableComponent) payableComponent: PayableComponent;
    @ViewChild(CommercialAddressListComponent) addressPartnerList: CommercialAddressListComponent;

    partnerId: string;
    partner: Partner;
    currency: string;

    isUpdated: boolean = false;
    isBranchSubCurrent: boolean;

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        private _activedRoute: ActivatedRoute,
        private _cd: ChangeDetectorRef,
        protected _ngProgressService: NgProgress,
        protected _sysFilemanagementRepo: SystemFileManageRepo,
        protected _store: Store<IAppState>
    ) {
        super(_router, _toastService, _catalogueRepo, _ngProgressService, _activedRoute, _sysFilemanagementRepo, _store);
    }

    ngOnInit(): void {
    }

    ngAfterViewInit() {
        this.contractList.partnerLocation = this.formCreate.partnerLocation.value;
        combineLatest([
            this._activedRoute.params,
            this._activedRoute.data,
        ]).pipe(
            map(([p, d]) => ({ ...p, ...d }))
        ).subscribe(
            (res: any) => {
                if (res.action) {
                    if (localStorage.getItem('success_add_sub') === "true") {
                        localStorage.removeItem('success_add_sub');
                        this.back();
                    }
                    this.isAddSubPartner = res.action;
                    this.partnerList.isAddSubPartner = this.isAddSubPartner;
                } else {
                    localStorage.removeItem('success_add_sub');
                }
                if (res.type) {
                    this.type = res.type;
                    this.partnerList.partnerType = this.type;
                }
                if (res.partnerId) {
                    this.partnerId = res.partnerId;
                    this.partnerList.parentId = this.partnerId;
                    this.formCommercialComponent.partnerId = this.partnerId;
                    this.getDetailCustomer(this.partnerId);
                    this.partnerEmailList.getEmailPartner(this.partnerId);
                    if (!this.isAddSubPartner) {
                        this.contractList.partnerId = this.partnerId;
                        this.partnerEmailList.partnerId = this.partnerId;
                        this.contractList.getListContract(this.partnerId);
                        this.partnerList.getSubListPartner(this.partnerId);
                    }
                    this.formBankList.partnerId = res.partnerId;
                    this.formBankList.getListBank(res.partnerId);

                    this.addressPartnerList.partnerId = res.partnerId;
                    this.addressPartnerList.getAddressPartner(res.partnerId);
                    this.addressPartnerList.partner = res;

                    this.payableComponent.partnerId = res.partnerId;
                    this.payableComponent.getFileContract(res.partnerId);
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
                    if (!!res) {
                        this.partner = res;
                        this.formBankList.partner = this.partner;
                        this.addressPartnerList.partner = this.partner;
                        this.formCommercialComponent.active = this.partner.active;
                        this.formCreate.isBranchSub = this.isAddSubPartner;
                        this.setDataForm(this.partner);
                        if (this.isAddSubPartner) {
                            this.formCreate.isUpdate = false;
                            this.formCreate.getACRefName(this.partner.id);
                        } else {
                            this.formCreate.acRefCustomers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, true, this.partner.id);
                            this.formCreate.getACRefName(this.partner.parentId);
                        }
                        // this.formCreate.partnerLocation.setValue([<CommonInterface.INg2Select>{ id: this.partner.partnerLocation, text: this.partner.partnerLocation }]);

                        this.formCreate.getShippingProvinces(res.countryShippingId);
                        this.formCreate.getBillingProvinces(res.countryId);
                        this.payableComponent.getGeneralPayable(this.partner.id, this.partner.currency === null ? "VND" : this.partner.currency);
                    }
                    else {
                        this.back();
                    }

                }
            );
    }

    setDataForm(partner: Partner) {
        this.formCreate.provinceShippingIdName = partner.provinceShippingName;
        this.formCreate.countryShippingIdName = partner.countryShippingName;
        this.formCreate.provinceIdName = partner.provinceName;
        this.formCreate.countryIdName = partner.countryName;
        this.formCreate.formGroup.patchValue({
            accountNo: this.isAddSubPartner ? null : partner.accountNo,
            partnerNameEn: this.isAddSubPartner ? null : partner.partnerNameEn,
            partnerNameVn: this.isAddSubPartner ? null : partner.partnerNameVn,
            shortName: this.isAddSubPartner ? null : partner.shortName,
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
            partnerLocation: partner.partnerLocation,
            parentId: this.isAddSubPartner ? partner.id : partner.parentId,
            bankAccountName: partner.bankAccountName,
            identityNo: partner.identityNo,
            dateId: !!partner.dateId ? { startDate: new Date(partner.dateId), endDate: new Date(partner.dateId) } : null,
            placeId: partner.placeId
        });
        if (this.formCommercialComponent.partnerId !== partner.parentId) {
            this.formCommercialComponent.isDisabled = false;
        }
        else {
            this.formCommercialComponent.isDisabled = true;
        }
        this.contractList.partnerLocation = partner.partnerLocation;
        this.payableComponent.payableForm.patchValue({
            paymentTerm: partner.paymentTerm,
            currency: partner.currency
        })
        this.formCommercialComponent.isBranchSubCurrent = !!partner.parentId && partner.id != partner.parentId;
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

    getSubListPartner(partnerId: string) {
        this._catalogueRepo.getSubListPartner(partnerId)
            .pipe(catchError(this.catchError), finalize(() =>
                this.partnerList.isLoading = false
            )).subscribe(
                (res: any[]) => {
                    this.partnerList.partners = res || [];
                    console.log(this.partnerList);
                }
            );
    }

    onSaveWithPayable(payable: any) {
        this.isUpdated = true;
        this.formCreate.isSubmitted = true;
        console.log(payable);

        if (!this.formCreate.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        if (!this.formCreate.parentId.value && !this.contractList.contracts.length) {
            this._toastService.warning("Partner don't have any contract in this period, Please check it again!");
            return;
        }

        const modelAdd: Partner = this.formCreate.formGroup.getRawValue();
        modelAdd.contracts = this.contractList.contracts;

        modelAdd.id = this.isAddSubPartner ? null : this.partnerId;
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
        //modelAdd.bankAccountName = this.partner.bankAccountName;
        modelAdd.bankAccountAddress = this.partner.bankAccountAddress;
        modelAdd.note = this.partner.note;
        modelAdd.public = this.partner.public;
        modelAdd.workPlaceId = this.partner.workPlaceId;
        modelAdd.partnerMode = this.partner.partnerMode;
        modelAdd.partnerLocation = this.formCreate.partnerLocation.value;
        //
        modelAdd.paymentTerm = payable.paymentTerm;
        modelAdd.currency = payable.currency;

        console.log(modelAdd);
        this.updatePartner(modelAdd);
        console.log(this.partner);

        //this.payableComponent.getGeneralPayable(this.partner.id, payable.currency);
    }

    onSaveDetail() {
        this.formCreate.isSubmitted = true;

        if (!this.formCreate.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        if (!this.formCreate.parentId.value && !this.contractList.contracts.length) {
            this._toastService.warning("Partner don't have any contract in this period, Please check it again!");
            return;
        }

        const modelAdd: Partner = this.formCreate.formGroup.getRawValue();
        modelAdd.contracts = this.contractList.contracts;

        modelAdd.id = this.isAddSubPartner ? null : this.partnerId;
        modelAdd.userCreated = this.partner.userCreated;
        modelAdd.datetimeCreated = this.partner.datetimeCreated;
        modelAdd.partnerType = this.partner.partnerType;
        modelAdd.partnerGroup = this.partner.partnerGroup;
        modelAdd.datetimeCreated = this.partner.datetimeCreated;
        modelAdd.dateId = (modelAdd.dateId === null || modelAdd.dateId?.startDate === null) ? null : (!!modelAdd.dateId ? (!!modelAdd.dateId.startDate ? formatDate(modelAdd.dateId.startDate, 'yyyy-MM-dd', 'en'):  formatDate(new Date(modelAdd.dateId), 'yyyy-MM-dd', 'en')) : null);

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
        //modelAdd.bankAccountName = this.partner.bankAccountName;
        modelAdd.bankAccountAddress = this.partner.bankAccountAddress;
        modelAdd.note = this.partner.note;
        modelAdd.public = this.partner.public;
        modelAdd.workPlaceId = this.partner.workPlaceId;
        modelAdd.partnerMode = this.partner.partnerMode;
        modelAdd.partnerLocation = this.formCreate.partnerLocation.value;


        console.log(modelAdd);
        this.updatePartner(modelAdd);
    }

    updateStatusPartner($event) {
        const obj = $event;
        if (obj.partnerStatus === true && (obj.isRequestApproval === false || obj.isRequestApproval === null)) {
            this.partner.active = true;
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
                        this.isAddSubPartner = false;
                        this.formCreate.formGroup.patchValue(res);
                    } else {
                        this._toastService.error(res.message);
                    }
                    if (this.isUpdated) {
                        this.payableComponent.getGeneralPayable(this.partner.id, body.currency);
                        this.isUpdated = false;
                    }
                },
                (error: HttpErrorResponse) => {
                    console.log(error);
                }
            );
    }

    gotoList() {
        localStorage.setItem('success_add_sub', "true");
        this.back();
    }
}

