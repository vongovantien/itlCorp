import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, ActivatedRoute } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { AppForm } from 'src/app/app.form';
import { CatalogueRepo } from '@repositories';
import { InfoPopupComponent, ConfirmPopupComponent } from '@common';
import { Partner } from '@models';

import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from '../components/contract/commercial-contract-list.component';
import {CommercialBranchSubListComponent} from '../components/branch-sub/commercial-branch-sub-list.component';

import { of } from 'rxjs';
import { catchError, concatMap, map } from 'rxjs/operators';
import { RoutingConstants } from '@constants';
import { isMoment } from 'moment';


@Component({
    selector: 'app-create-commercial',
    templateUrl: './create-commercial.component.html',
})
export class CommercialCreateComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormCreateComponent, { static: false }) formCreate: CommercialFormCreateComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) contractList: CommercialContractListComponent;
    @ViewChild(CommercialBranchSubListComponent, { static: false }) partnerList: CommercialBranchSubListComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild('taxCodeInfo', { static: false }) infoPopupTaxCode: InfoPopupComponent;
    @ViewChild('internalReferenceConfirmPopup', { static: false }) confirmTaxcode: ConfirmPopupComponent;

    invalidTaxCode: string;

    fileList: File[] = [];
    type: string;

    constructor(
        protected _router: Router,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        protected _ngProgressService: NgProgress,
        protected _activeRoute: ActivatedRoute
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();

    }

    ngOnInit(): void {
        this._activeRoute.data.subscribe((result: { name: string, type: string }) => {
            this.type = result.type;
        });
    }

    gotoList() {
        if (this.type === 'Customer') {
            this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}`]);
        } else {
            this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}`]);
        }
    }

    onSave() {
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

        modelAdd.partnerMode = 'External';
        modelAdd.partnerLocation = this.formCreate.partnerLocation.value[0].id;
        modelAdd.partnerType = this.type;
        this.type === 'Customer' ? modelAdd.partnerGroup = 'CUSTOMER' : modelAdd.partnerGroup = 'CUSTOMER; AGENT';
        modelAdd.contracts = [...this.contractList.contracts];


        this.saveCustomerCommercial(modelAdd);
    }

    saveCustomerCommercial(body: Partner) {
        const bodyValidateTaxcode = {
            id: body.id,
            taxCode: body.taxCode,
            internalReferenceNo: body.internalReferenceNo
        };
        this._catalogueRepo.checkTaxCode(bodyValidateTaxcode).pipe(
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
            concatMap(
                (v) => {
                    if (v === false) {
                        return of(false);
                    }
                    // * Ngược lại không trùng TaxCode thì đi tạo Partner.
                    return this._catalogueRepo.createPartner(body).pipe(
                        catchError((err, caught) => this.catchError),
                        concatMap((res: any) => {
                            if (res.result.success) {
                                this._toastService.success("New data added");
                                this.contractList.contracts.forEach(element => {
                                    if (!!element.fileList) {
                                        this.fileList.push(element.fileList[0]);
                                    }
                                });

                                let i = 0;
                                for (const obj of this.contractList.contracts) {
                                    obj.id = res.model.idsContract[i];
                                    i++;
                                }
                                const idsContract: any = [];
                                this.contractList.contracts.forEach(element => {
                                    if (!!element.fileList) {
                                        idsContract.push(element.id);
                                    }
                                });
                                // Nếu không có file thì return luôn.
                                if (this.fileList.length === 0) {
                                    return of({ status: true, id: res.model.id });
                                }
                                return this._catalogueRepo.uploadFileMoreContract(idsContract, res.model.id, this.fileList).pipe(
                                    catchError((err, caught) => this.catchError),
                                    concatMap((rs: any) => {
                                        if (!!rs) {
                                            this._toastService.success("Upload file successfully!");
                                            return of(res.model.id);
                                        }
                                    })
                                );
                            } else {
                                this._toastService.error("Opps", "Something getting error!");
                            }
                            // if (res.status) {
                            //     if (res.data) {
                            //         this._toastService.success(res.message);

                            //     }
                            // }
                        })
                    );
                })
        ).subscribe(
            (res) => {
                if (res === false) {
                    //this.infoPopupTaxCode.show();
                    this.formCreate.isExistedTaxcode = true;
                    return;
                }
                if (res.status === true) {
                    if (this.type === 'Customer') {
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${res.id}`]);
                    } else {
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${res.id}`]);
                    }
                } else if (!!res) {
                    if (this.type === 'Customer') {
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.CUSTOMER}/${res}`]);
                    } else {
                        this._router.navigate([`${RoutingConstants.COMMERCIAL.AGENT}/${res}`]);
                    }
                }
            },
            (err) => {
                console.log(err);
            }
        );
    }

    onFocusInternalReference() {
        this.confirmTaxcode.hide();
        //
        this.formCreate.handleFocusInternalReference();
    }
}

export interface IValidateTaxCode {
    id: string;
    taxCode: string;
    internalReferenceNo: string;
}
