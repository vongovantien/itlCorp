import { Component, OnInit, ViewChild, Output, EventEmitter, Input } from '@angular/core';
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
    @Output() onCreate = new EventEmitter();

    saleMans: Saleman[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    status: CommonInterface.ICommonTitleValue[] = [];
    offices: any[] = [];
    selectedService: any = {};
    saleManToAdd: Saleman = new Saleman();
    strSalemanCurrent: any = {};
    strOfficeCurrent: any = '';
    selectedStatus: any = {};
    users: any[] = [];
    isSave: boolean = false;
    isExistedSaleman: boolean = false;
    isDup: boolean = false;
    saleManToView: Saleman = new Saleman();
    isDetail: boolean = false;
    selectedDataSaleMan: any;
    selectedDataOffice: any;

    @Input() popupData: Saleman;


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
        console.log(this.saleManToView);
    }

    showSaleman(saleman: Saleman) {
        this.isDetail = true;
        this.saleManToView = saleman;
        console.log(this.saleManToView);
    }

    resetForm() {
        this.strSalemanCurrent = {};
        this.strOfficeCurrent = {};
        this.saleManToAdd = new Saleman();
        this.saleManToAdd.saleman_ID = {};
        this.saleManToAdd.office = {};

    }
    closePoup() {
        this.isDetail = false;
        this.hide();

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
        this.OnCreate();

        // if (this.saleManToAdd.saleman_ID.length > 0 && this.saleManToAdd.office.length > 0) {
        //     this.OnCreate();
        // if (!this.isDup) {
        //     this.onCreate.emit(this.saleManToAdd);
        //     this.hide();
        // }

        // }

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

    onSelectSaleMan(saleMan: any) {
        this.strSalemanCurrent = { field: 'username', value: saleMan.username };
        this.selectedDataSaleMan = saleMan;
    }



    onSelectOffice(office: any) {
        this.strOfficeCurrent = { field: 'abbrCompany', value: office.abbrCompany };
        this.selectedDataOffice = office;
    }
    OnCreate() {
        const salemaneffectdate = this.saleManToAdd.effectDate == null ? null : formatDate(this.saleManToAdd.effectDate.startDate, 'yyyy-MM-dd', 'en');

        const saleMane: any = {
            company: this.saleManToAdd.office,
            office: this.saleManToAdd.office,
            effectDate: salemaneffectdate,
            status: this.selectedStatus.value,
            partnerId: null,
            saleman_ID: this.selectedDataSaleMan.username,
            service: this.selectedService.id,
            createDate: new Date()
        };
        this.saleManToAdd = new Saleman(saleMane);
        this.onCreate.emit(this.saleManToAdd);
    }
}
