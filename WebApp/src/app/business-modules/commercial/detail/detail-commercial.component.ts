import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';

import { CommercialCreateComponent } from '../create/create-commercial.component';

import { finalize, catchError, concatMap, map } from 'rxjs/operators';
import { of, combineLatest, Observable } from 'rxjs';
import { ConfirmPopupComponent } from '@common';
import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialBranchSubListComponent } from '../components/branch-sub/commercial-branch-sub-list.component';
import { CommonEnum } from '@enums';
import { RoutingConstants } from '@constants';

@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})
export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {
    @ViewChild(CommercialFormCreateComponent, { static: false }) formCommercialComponent: CommercialFormCreateComponent;
    @ViewChild('internalReferenceConfirmPopup', { static: false }) confirmTaxcode: ConfirmPopupComponent;
    @ViewChild(CommercialBranchSubListComponent, { static: false }) formBranchSubList: CommercialBranchSubListComponent;
    partnerId: string;
    partner: Partner;
    isAddSub: boolean;

    public originRoute: string = '';
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

    ngOnInit(): void {
        if (!localStorage.getItem('id_token_url_obj') && this._router.url.search('BranchSub') === -1) {
            localStorage.setItem('id_token_url_obj', this._router.url);
        }
    }

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
                    this.partnerList.partnerType = this.type;
                }
                if (res.name) {
                    this.isAddSub = res.name === 'New Branch/Sub';
                }
                if (res.partnerId) {
                    this.partnerId = res.partnerId;
                    this.partnerList.parentId = this.partnerId;
                    this.partnerList.isAddSubPartner = this.isAddSub;

                    this.getDetailCustomer(this.partnerId);
                    if (!this.isAddSub) {
                        this.contractList.partnerId = this.partnerId;
                        this.contractList.getListContract(this.partnerId);
                        this.partnerList.getSubListPartner(this.partnerId, this.type);
                    }
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
                    if (this.isAddSub) {
                        this.formCreate.isBranchSub = true;
                        this.formCreate.isUpdate = false;
                        this.formCreate.getACRefName(this.partner.id);
                    } else {
                        this.formCreate.acRefCustomers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.ALL, true, this.partner.id);
                        this.formCreate.getACRefName(this.partner.parentId);
                    }
                    // this.formCreate.partnerLocation.setValue([<CommonInterface.INg2Select>{ id: this.partner.partnerLocation, text: this.partner.partnerLocation }]);

                    this.formCreate.getShippingProvinces(res.countryShippingId);
                    this.formCreate.getBillingProvinces(res.countryId);
                }
            );
    }

    setDataForm(partner: Partner) {
        this.formCreate.formGroup.patchValue({
            accountNo: this.isAddSub ? null : partner.accountNo,
            partnerNameEn: partner.partnerNameEn,
            partnerNameVn: partner.partnerNameVn,
            shortName: partner.shortName,
            taxCode: this.isAddSub ? null : partner.taxCode,
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
            parentId: this.isAddSub ? partner.id : partner.parentId
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

    getSubListPartner(partnerId: string, partnerType: string) {
        this._catalogueRepo.getSubListPartner(partnerId, partnerType)
            .pipe(catchError(this.catchError), finalize(() =>
                this.partnerList.isLoading = false
            )).subscribe(
                (res: any[]) => {
                    this.partnerList.partners = res || [];
                    console.log(this.partnerList);
                }
            );
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

        modelAdd.id = this.isAddSub ? null : this.partnerId;
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
        modelAdd.partnerLocation = this.formCreate.partnerLocation.value[0].id;

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
                        this.isAddSub = false;
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

    gotoList() {
        this.originRoute = localStorage.getItem('id_token_url_obj');
        if (this.originRoute === this._router.url) {
            localStorage.removeItem('id_token_url_obj');
            if (this.type === 'Customer') {
                this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}`]);
            } else {
                this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}`]);
            }
        } else
            if (this.isAddSub) {
                this.partnerId = this.partner.id;
                if (this.type === 'Customer') {
                    this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${this.partnerId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${this.partnerId}`]);
                }
            } else {
                this.partnerId = this.partner.parentId;
                if (this.type === 'Customer') {
                    this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${this.partnerId}`]);
                } else {
                    this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${this.partnerId}`]);
                }
            }
    }

}

