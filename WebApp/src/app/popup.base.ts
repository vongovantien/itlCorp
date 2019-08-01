import { ModalOptions, ModalDirective } from "ngx-bootstrap";
import { AppPage } from "src/app/app.base";
import { ViewChild } from "@angular/core";

export abstract class PopupBase extends AppPage {
    @ViewChild("popup", { static: false }) popup: ModalDirective;

    options: ModalOptions = {
        animated: false,
        keyboard: true
    };

    constructor() {
        super();
    }

    // * fn set options
    setOptions(options?: ModalOptions) {
        const self = this;
        if (typeof options === 'object') {
            for (const key in options) {
                if (self.hasOwnProperty(key)) {
                    self[key] = options[key];
                }
            }
        }
    }

    // * show poup
    show(options?: ModalOptions): void {
        this.setOptions(Object.assign(this.options, options));
        if (!this.popup.isShown) {
            this.popup.config = this.options;
            this.popup.show();
        }
    }

    hide() {
        this.popup.hide();
    }

    // event fire when hide popup
    onHide($event: any) {
    }

    // event fire when show popup
    onShow($event: any) {
    }
}
