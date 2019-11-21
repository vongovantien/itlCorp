import { Component, OnInit } from '@angular/core';
import { ButtonModalSetting } from 'src/app/shared/models/layout/button-modal-setting.model';
import { ButtonType } from 'src/app/shared/enums/type-button.enum';

@Component({
    selector: 'app-sea-fcl-import-manifest',
    templateUrl: './sea-fcl-import-manifest.component.html'
})
export class SeaFclImportManifestComponent implements OnInit {
    saveButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.save
    };

    cancelButtonSetting: ButtonModalSetting = {
        typeButton: ButtonType.cancel
    };
    constructor() { }

    ngOnInit() {

    }

}
