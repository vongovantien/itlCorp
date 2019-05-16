import { Component, OnInit } from '@angular/core';
import { BaseService } from 'src/services-base/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { EcusConnection } from 'src/app/shared/models/tool-setting/ecus-connection';
import {prepareNg2SelectData} from 'src/helper/data.helper';
import { NgForm } from '@angular/forms';
import { async } from '@angular/core/testing';
declare var $: any;
@Component({
    selector: 'app-ecus-connection',
    templateUrl: './ecus-connection.component.html',
    styleUrls: ['./ecus-connection.component.sass']
})
export class EcusConnectionComponent implements OnInit {

    EcusConnectionAdd: EcusConnection = new EcusConnection();
    Users : any[] =[];
    constructor(private baseService: BaseService,private api_menu: API_MENU) { }

    ngOnInit() {
        this.getListUsers();
    }

    getListUsers(){
        this.baseService.get(this.api_menu.System.User_Management.getAll).subscribe((data:any)=>{
            this.Users = prepareNg2SelectData(data,"id","username");
            console.log(this.Users);
        });
    }
    SubmitNewConnect(form:NgForm){
        var error = $('#add-connection-modal').find('div.has-danger');
        console.log(this.EcusConnectionAdd);
        setTimeout(async() => {
            if(form.submitted && error.length===0){
                var res = await this.baseService.postAsync(this.api_menu.ToolSetting.EcusConnection.addNew,this.EcusConnectionAdd);
                if(res.status){
                    this.resetDisplay();
                    $('#add-connection-modal').modal('hide');
                }
            }
        }, 300);

    }

    isDisplay:boolean = true;
    resetDisplay(){
      this.isDisplay = false;
      setTimeout(() => {
        this.isDisplay = true;
      }, 30);
    }

    /**
   * ng2-select
   */
    public items: Array<string> = ['Option 1', 'Option 2', 'Option 3', 'Option 4',
        'Option 5', 'Option 6', 'Option 7', 'Option 8', 'Option 9', 'Option 10',];

    private value: any = {};
    private _disabledV: string = '0';
    public disabled: boolean = false;

    private get disabledV(): string {
        return this._disabledV;
    }

    private set disabledV(value: string) {
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
