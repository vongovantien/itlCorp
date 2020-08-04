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


@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})
export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {

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

    getDetailCustomer(partnerId: string) {
        this._catalogueRepo.getDetailPartner(partnerId)
            .subscribe(
                (res: Partner) => {
                    this.partner = res;
                    console.log("detail partner:", this.partner);
                    this.formCreate.formGroup.patchValue(res);
                    this.formCreate.getShippingProvinces(res.countryShippingId);
                    this.formCreate.getBillingProvinces(res.countryId);
                }
            );
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
        modelAdd.partnerLocation = this.partner.partnerLocation;

        console.log(modelAdd);
        this.updatePartner(modelAdd);
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
                        } else {
                            this.invalidTaxCode = `This <b>Taxcode</b> already <b>Existed</b> in  <b>${value.shortName}</b>, If you want to Create Internal account, Please fill info to <b>Internal Reference Info</b>.`;
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
                        this.infoPopupTaxCode.show();
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

