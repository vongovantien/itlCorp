import { Component, OnInit, ViewChild } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Router, ActivatedRoute } from '@angular/router';
import { CatalogueRepo } from '@repositories';
import { ToastrService } from 'ngx-toastr';
import { InfoPopupComponent } from '@common';

import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from '../components/contract/commercial-contract-list.component';
import { Partner } from '@models';
import { catchError, finalize, concatMap } from 'rxjs/operators';
import { NgProgress } from '@ngx-progressbar/core';
import { of } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'app-create-commercial',
    templateUrl: './create-commercial.component.html',
})
export class CommercialCreateComponent extends AppForm implements OnInit {

    @ViewChild(CommercialFormCreateComponent, { static: false }) formCreate: CommercialFormCreateComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) contractList: CommercialContractListComponent;
    @ViewChild(InfoPopupComponent, { static: false }) infoPopup: InfoPopupComponent;
    @ViewChild('taxCodeInfo', { static: false }) infoPopupTaxCode: InfoPopupComponent;


    invalidTaxCode: string;

    fileList: any[] = [];
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
            console.log(result);
            this.type = result.type;
            console.log(this.type);
        })
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
        modelAdd.partnerType = this.type;
        this.type === 'Customer' ? modelAdd.partnerGroup = 'CUSTOMER' : modelAdd.partnerGroup = 'CUSTOMER;AGENT';
        modelAdd.contracts = [...this.contractList.contracts];


        this.saveCustomerCommercial(modelAdd);
    }

    saveCustomerCommercial(body) {
        this._catalogueRepo.createPartner(body)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete()),
                concatMap((res: CommonInterface.IResult) => {
                    if (res.status) {
                        if (res.data) {
                            console.log(res.data);
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
                            if (this.fileList.length === 0) {
                                if (this.type === 'Customer') {
                                    this._router.navigate(["/home/commercial/customer"]);
                                } else {
                                    this._router.navigate(["/home/commercial/agent"]);

                                }
                            }
                            return this._catalogueRepo.uploadFileMoreContract(idsContract, res.data.id, this.fileList);
                        }
                    }
                    return of({ data: null, message: 'Something getting error. Please check again!', status: false });
                })
            ).subscribe(
                (res: any) => {
                    if (this.type === 'Customer') {
                        this._router.navigate(["/home/commercial/customer"]);
                    } else {
                        this._router.navigate(["/home/commercial/agent"]);

                    }

                },
                (error: HttpErrorResponse) => {
                    console.log(error);
                }
            );
    }
}

export interface IValidateTaxCode {
    id: string;
    taxCode: string;
    internalReferenceNo: string;
}
