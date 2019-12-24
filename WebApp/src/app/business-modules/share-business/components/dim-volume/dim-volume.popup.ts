import { Component, OnInit, ViewChild } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { DIM } from '@models';
import { ConfirmPopupComponent } from '@common';

@Component({
    selector: 'dim-volume-popup',
    templateUrl: './dim-volume.popup.html'
})

export class ShareBusinessDIMVolumePopupComponent extends PopupBase implements OnInit {

    dims: DIM[] = [];

    constructor() {
        super();
    }

    ngOnInit() { }

    addDIM() {
        this.dims.push(new DIM());
    }

    delete(index: number) {
        this.dims.splice(index, 1);
    }

    saveDIM() {
        console.log(this.dims);
    }
}
