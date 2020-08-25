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

import { of } from 'rxjs';
import { catchError, concatMap, map } from 'rxjs/operators';


@Component({
    selector: 'app-create-commercial',
    templateUrl: './create-commercial.component.html',
})
export class CommercialCreateComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormCreateComponent, { static: false }) formCreate: CommercialFormCreateComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) contractList: CommercialContractListComponent;
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
            this._router.navigate(["home/commercial/customer"]);
        } else {
            this._router.navigate(["home/commercial/agent"]);
        }
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

        modelAdd.partnerMode = 'External';
        modelAdd.partnerLocation = 'Domestic';
        modelAdd.partnerType = this.type;
        this.type === 'Customer' ? modelAdd.partnerGroup = 'CUSTOMER' : modelAdd.partnerGroup = 'CUSTOMER;AGENT';
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
                        concatMap((res: CommonInterface.IResult) => {
                            if (res.status) {
                                if (res.data) {
                                    this._toastService.success(res.message);
                                    this.contractList.contracts.forEach(element => {
                                        if (!!element.fileList) {
                                            this.fileList.push(element.fileList[0]);
                                        }
                                    });

                                    let i = 0;
                                    for (const obj of this.contractList.contracts) {
                                        obj.id = res.data.idsContract[i];
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
                                        return of(true);
                                    }
                                    return this._catalogueRepo.uploadFileMoreContract(idsContract, res.data.id, this.fileList);
                                }
                            }
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
                if (res || res.status) {
                    if (this.type === 'Customer') {
                        this._router.navigate(["/home/commercial/customer"]);
                    } else {
                        this._router.navigate(["/home/commercial/agent"]);
                    }
                }
                console.log(res);
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
