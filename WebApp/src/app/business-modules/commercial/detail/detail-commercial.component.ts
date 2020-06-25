import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';

import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { CatalogueRepo } from '@repositories';
import { Partner } from '@models';

import { CommercialCreateComponent } from '../create/create-commercial.component';


import { tap, switchMap, finalize, catchError, concatMap } from 'rxjs/operators';
import { of } from 'rxjs';
import _merge from 'lodash/merge';
import { HttpErrorResponse } from '@angular/common/http';
import { FormContractCommercialPopupComponent } from '../../share-commercial-catalogue/components/form-contract-commercial-catalogue.popup';


@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})
export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {

    partnerId: string;
    partner: Partner;

    invalidTaxCode: string;

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        private _activedRoute: ActivatedRoute,
        private _cd: ChangeDetectorRef,
        protected _ngProgressService: NgProgress,
    ) {
        super(_router, _toastService, _catalogueRepo, _ngProgressService);
    }

    ngOnInit(): void { }

    ngAfterViewInit() {

        this._activedRoute.params.pipe(
            tap((param: Params) => {
                this.partnerId = !!param.partnerId ? param.partnerId : '';

            }),
            switchMap(() => of(this.partnerId)),
        ).subscribe(
            (partnerId: string) => {
                if (partnerId) {
                    this.contractList.partnerId = partnerId;
                    this.getDetailCustomer(partnerId);
                    this.contractList.getListContract(partnerId);
                } else {
                    this.gotoList();
                }
            }
        );
        this._cd.detectChanges();
    }

    getDetailCustomer(partnerId: string) {
        this._catalogueRepo.getDetailPartner(partnerId)
            .subscribe(
                (res: Partner) => {
                    this.partner = res;
                    console.log("detail partner:", this.partner);
                    this.formCreate.formGroup.patchValue(res);
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

    onSubmitData() {
        const formBody = this.formCreate.formGroup.getRawValue();
    }

    onSave() {
        this.formCreate.isSubmitted = true;

        if (!this.formCreate.formGroup.valid) {
            this.infoPopup.show();
            return;
        }

        const modelAdd: Partner = this.formCreate.formGroup.getRawValue();
        // modelAdd.saleMans = this.contractList.contracts;  // TODO implement contract;

        modelAdd.id = this.partnerId;
        modelAdd.userCreated = this.partner.userCreated;
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


        console.log(modelAdd);
        // this.updatePartner(body);
    }

    updatePartner(body: Partner) {
        this._catalogueRepo.checkTaxCode(this.partner)
            .pipe(
                tap(),
                switchMap(() => (responseValidateTaxCode: any) => {
                    if (!!responseValidateTaxCode) {
                        this.setError(this.formCreate.formGroup.controls["internalReferenceNo"], { taxCode: true });
                    }
                    return this._catalogueRepo.updatePartner(body.id, body)
                        .pipe(
                            catchError(this.catchError),
                            finalize(() => this._progressRef.complete()),
                            concatMap((data: CommonInterface.IResult) => {
                                if (data.status) {
                                    this._toastService.success(data.message);
                                    return this._catalogueRepo.getDetailPartner(body.id);
                                }
                                return of({ data: null, message: 'Something getting error. Please check again!', status: false });
                            })
                        );
                })
            )
            .subscribe(
                (res: any) => {
                    console.log(res);
                },
                (error: HttpErrorResponse) => {
                    console.log(error);
                }
            );
    }
}

