import { Component, OnInit } from '@angular/core';

@Component({
    selector: 'app-shipping-instruction',
    templateUrl: './shipping-instruction.component.html',
    styleUrls: ['./shipping-instruction.component.scss']
})
export class ShippingInstructionComponent implements OnInit {

    constructor() { }

    ngOnInit() {
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
