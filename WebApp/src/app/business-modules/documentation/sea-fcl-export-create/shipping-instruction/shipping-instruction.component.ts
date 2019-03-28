import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';

@Component({
    selector: 'app-shipping-instruction',
    templateUrl: './shipping-instruction.component.html',
    styleUrls: ['./shipping-instruction.component.scss']
})
export class ShippingInstructionComponent implements OnInit {
    userInCharges: any[] = [];
    suppliers: any[] = [];

    constructor(private baseServices: BaseService,
        private api_menu: API_MENU) { }

    async ngOnInit() {
        await this.getUserInCharges(null);
        await this.getSuppliers(null);
    }
    async getUserInCharges(searchText: any) {
        const users = await this.baseServices.getAsync(this.api_menu.System.User_Management.getAll, false, false);
        if (users != null) {
            this.userInCharges = users;
            console.log(this.userInCharges);
        }
        else{
            this.userInCharges = [];
        }
    }
    async getSuppliers(searchText: any) {
        let criteriaSearchAgent = { partnerGroup: PartnerGroupEnum.CARRIER, inactive: false, all: searchText };
        // if(this.shipment.id != "00000000-0000-0000-0000-000000000000"){
        //     criteriaSearchAgent.inactive = null;
        // }
        const partners = await this.baseServices.postAsync(this.api_menu.Catalogue.PartnerData.paging + "?page=1&size=20", criteriaSearchAgent, false, false);
        if (partners != null) {
            this.suppliers = partners.data;
            console.log(this.suppliers);
        }
        else{
            this.suppliers = [];
        }
    }
    public paymentTypes: Array<string> = ['Prepaid', 'Collect'];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    public get disabledV(): string {
        return this._disabledV;
    }

    public set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === '1';
    }

    public selected(value: any): void {
        console.log('Selected value is: ', value);
    }

    public removed(value: any): void {
        console.log('Removed value is: ', value);
    }

    public typed(value: any): void {
        console.log('New search input: ', value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }
}
