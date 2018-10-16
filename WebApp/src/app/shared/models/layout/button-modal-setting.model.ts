import { ButtonType } from "../../enums/type-button.enum";

export class ButtonModalSetting {
    buttonAttribute?: ButtonAttributeSetting;
    typeButton: ButtonType;
    dataTarget?: string;
}
export class ButtonAttributeSetting {
    titleButton: string;
    classStyle: string;
    //targetModal?: string;
    icon?: string;
}