import { Component, OnInit, ViewChild, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseService } from 'src/app/shared/services/base.service';
import { ToastrService } from 'ngx-toastr';
import { API_MENU } from 'src/constants/api-menu.const';
import { NgForm } from '@angular/forms';
import { SortService } from 'src/app/shared/services/sort.service';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError } from "rxjs/operators";
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';
import { PopupBase } from 'src/app/popup.base';
import { formatDate } from '@angular/common';
@Component({
    selector: 'app-saleman-popup',
    templateUrl: './saleman-popup.component.html',
})

export class SalemanPopupComponent extends PopupBase {
    @Output() isCloseModal = new EventEmitter();
    saleMandetail: any[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    status: CommonInterface.ICommonTitleValue[] = [];
    offices: any[] = [];
    selectedService: any = {};
    saleManToAdd: Saleman = new Saleman();
    strSalemanCurrent: any = '';
    strOfficeCurrent: any = '';
    selectedStatus: any = {};
    users: any[] = [];
    isSave: boolean = false;


    constructor(
        private baseService: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private _catalogueRepo: CatalogueRepo,
        private sortService: SortService,
        private toastr: ToastrService,
    ) {

        super();
    }

    ngOnInit() {
        this.getComboboxData();
    }
    getComboboxData(): any {
        this.getSalemans();
        this.getService();
        this.getOffice();
        this.status = this.getStatus();
        this.selectedStatus = this.status[1];

    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.selectedService = this.services[0];
                    }
                },
            );
    }

    ApplyToList() {
        this.isSave = true;
        if (this.strSalemanCurrent.length > 0 && this.strOfficeCurrent.length > 0) {
            this.OnCreate();
            console.log(this.saleManToAdd);
            this.isCloseModal.emit(this.saleManToAdd);
            this.hide();
        }

    }

    getOffice() {
        this._catalogueRepo.getListBranch()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.offices = res;
                    }
                },
            );
    }

    getStatus(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false },
        ];
    }

    async getSalemans() {
        let responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll);
        if (responses != null) {
            this.users = responses;
        }
    }

    OnCreate() {
        let salemaneffectdate = this.saleManToAdd.effectDate == null ? null : formatDate(this.saleManToAdd.effectDate.startDate, 'yyyy-MM-dd', 'en');
        this.saleManToAdd = new Saleman();
        this.saleManToAdd.company = this.strOfficeCurrent;
        this.saleManToAdd.office = this.strOfficeCurrent;
        this.saleManToAdd.effectDate = salemaneffectdate;
        this.saleManToAdd.status = this.selectedStatus.value;
        this.saleManToAdd.partnerId = null;
        this.saleManToAdd.saleman_ID = this.strSalemanCurrent;
        this.saleManToAdd.service = this.selectedService.id;
    }

}
