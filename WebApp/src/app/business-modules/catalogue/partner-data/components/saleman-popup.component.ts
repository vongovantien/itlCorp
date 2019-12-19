import { Component, Output, EventEmitter, Input } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError } from "rxjs/operators";
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';
import { PopupBase } from 'src/app/popup.base';
import { formatDate } from '@angular/common';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';

@Component({
    selector: 'app-saleman-popup',
    templateUrl: './saleman-popup.component.html',
})

export class SalemanPopupComponent extends PopupBase {
    @Output() onCreate = new EventEmitter();
    @Output() onDelete = new EventEmitter();
    saleMans: Saleman[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    statuss: any[] = [];
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
    index: number = 0;
    currrently_user: string = '';
    form: FormGroup;
    saleman: AbstractControl;
    service: AbstractControl;
    office: AbstractControl;
    status: AbstractControl;
    effectiveDate: AbstractControl;
    description: AbstractControl;

    @Input() popupData: Saleman;
    constructor(
        private baseService: BaseService,
        private api_menu: API_MENU,
        private _catalogueRepo: CatalogueRepo,
        private _fb: FormBuilder
    ) {

        super();
    }

    ngOnInit() {
        this.getComboboxData();
        this.currrently_user = localStorage.getItem('currently_userName');
        this.form = this._fb.group({
            saleman: [null, Validators.required],
            office: [null, Validators.required],
            status: [this.statuss[1]],
            effectiveDate: [],
            description: [],
            service: [this.services[0]],

        });
        this.saleman = this.form.controls["saleman"];
        this.service = this.form.controls["service"];
        this.office = this.form.controls["office"];
        this.status = this.form.controls["status"];
        this.effectiveDate = this.form.controls["effectiveDate"];
        this.description = this.form.controls["description"];
    }

    showSaleman(sm: Saleman) {
        this.form.reset();
        console.log(sm);
        this.form.setValue({
            office: sm.office,
            effectiveDate: !!sm.effectDate ? { startDate: new Date(sm.effectDate), endDate: new Date(sm.effectDate) } : null,
            service: this.services.filter(i => i.id === sm.service)[0],
            status: this.statuss.filter(i => i.title === sm.status)[0],
            saleman: sm.saleman_ID,
            description: sm.description
        });
    }

    onDeleteSaleman() {
        this.isDetail = false;
        this.onDelete.emit(this.index);
        this.hide();
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
        this.statuss = this.getStatus();
        // this.selectedStatus = this.statuss[1];

    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.service.setValue(this.services.filter(i => i.value === res.value)[0]);
                    }
                },
            );
    }

    ApplyToList() {
        this.isSave = true;
        this.OnCreate();
    }

    getOffice() {
        this._catalogueRepo.getListBranch()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.offices = res;
                        console.log(this.offices);
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
        const responses = await this.baseService.getAsync(this.api_menu.System.User_Management.getAll);
        if (responses != null) {
            this.users = responses;
        }
    }

    onSelectSaleMan(saleMan: any) {
        this.saleman.setValue(saleMan.id);
        this.selectedDataSaleMan = saleMan;
    }

    onSelectOffice(office: any) {
        this.office.setValue(office.abbrCompany);
        this.selectedDataOffice = office;
    }

    OnCreate() {
        this.isSave = true;
        if (this.form.valid) {
            const saleMane: any = {
                company: this.office.value,
                office: this.office.value,
                effectDate: !!this.effectiveDate.value && this.effectiveDate.value.startDate != null ? formatDate(this.effectiveDate.value.startDate !== undefined ? this.effectiveDate.value.startDate : this.effectiveDate.value, 'yyyy-MM-dd', 'en') : null,
                status: this.status.value.value,
                partnerId: null,
                saleman_ID: this.saleman.value,
                service: this.service.value.id,
                createDate: new Date()
            };
            this.saleManToAdd = new Saleman(saleMane);
            this.onCreate.emit(this.saleManToAdd);
        }
    }
}
