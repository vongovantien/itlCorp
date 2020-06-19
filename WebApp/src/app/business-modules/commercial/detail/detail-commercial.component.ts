import { Component, OnInit, ChangeDetectorRef, ViewChild } from '@angular/core';
import { Router, ActivatedRoute, Params } from '@angular/router';

import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';

import { DocumentationRepo, CatalogueRepo } from '@repositories';
import { Partner, Saleman } from '@models';

import { CommercialCreateComponent } from '../create/create-commercial.component';
import { CommercialFormCreateComponent } from '../components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from '../components/contract/commercial-contract-list.component';

import { tap, switchMap, finalize } from 'rxjs/operators';
import { of } from 'rxjs';
import _merge from 'lodash/merge';


@Component({
    selector: 'app-detail-commercial',
    templateUrl: './detail-commercial.component.html',
})
export class CommercialDetailComponent extends CommercialCreateComponent implements OnInit {

    @ViewChild(CommercialFormCreateComponent, { static: false }) formCreate: CommercialFormCreateComponent;
    @ViewChild(CommercialContractListComponent, { static: false }) contractList: CommercialContractListComponent;

    partnerId: string;
    partner: Partner;

    constructor(
        protected _router: Router,
        protected _documentRepo: DocumentationRepo,
        protected _toastService: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        private _activedRoute: ActivatedRoute,
        private _ngProgressService: NgProgress,
        private _cd: ChangeDetectorRef
    ) {
        super();
        this._progressRef = this._ngProgressService.ref();
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
                    this.getListContract(partnerId);
                } else {
                    this.gotoList();
                }
            }
        );
    }

    getDetailCustomer(partnerId: string) {
        this._catalogueRepo.getDetailPartner(partnerId)
            .subscribe(
                (res: Partner) => {
                    this.partner = res;
                    this.formCreate.formGroup.patchValue(res);
                }
            );
    }

    getListContract(partneId: string) {
        this.contractList.isLoading = true;
        this._catalogueRepo.getListContract(partneId)
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

    }

    gotoList() {
        this._router.navigate(["home/commercial/customer"]);
    }
}
