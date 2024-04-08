using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using ADODB;
using LSExtensionWindowLib;
using LSSERVICEPROVIDERLib;
using Patholab_Common;
using Patholab_DAL_V1;
using Patholab_DAL_V1.Enums;
using Patholab_XmlService;
using Patholab_Controls;


using MessageBox = System.Windows.MessageBox;
using System.Data;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace OtherPaymentsPages
{
    /// <summary>
    /// Interaction logic for MasterPage.xaml
    /// </summary>
    public partial class MasterPage : UserControl
    {

        public MasterPage()
        {
            InitializeComponent();
            TxtPathoLabName.Text = "";
            TxtPathoLabName.Focus();
            FirstFocus();
        }

        private void FirstFocus()
        {
            //First focus because nautius's bag
            _timerFocus = new Timer { Interval = 10000 };
            _timerFocus.Interval = 1000;
            _timerFocus.Tick += timerFocus_Tick;
            _timerFocus.Start();

        }

        private void timerFocus_Tick(object sender, EventArgs e)
        {
            TxtPathoLabName.Focus();

            _timerFocus.Stop();

        }

        public MasterPage(INautilusServiceProvider sp, INautilusProcessXML xmlProcessor, INautilusDBConnection _ntlsCon,
                          IExtensionWindowSite2 _ntlsSite, INautilusUser _ntlsUser)
        {




            if (_ntlsUser.GetRoleName().ToUpper() == "DEBUG") Debugger.Launch();
            InitializeComponent();
            //     this.SetResourceReference(Control.BackgroundProperty, System.Drawing.Color.FromName("Control"));
            this.sp = sp;
            this.xmlProcessor = xmlProcessor;
            this._ntlsCon = _ntlsCon;
            this._ntlsSite = _ntlsSite;
            this._ntlsUser = _ntlsUser;
        }


        #region Private fields

        private INautilusProcessXML xmlProcessor;
        private INautilusUser _ntlsUser;
        private IExtensionWindowSite2 _ntlsSite;
        private INautilusServiceProvider sp;
        private INautilusDBConnection _ntlsCon;
        private DataLayer dal;
        public List<U_PARTS> Parts;
        public bool DEBUG;
        private List<PHRASE_ENTRY> RotherStatus;
        private SDG_USER sdg;
        private U_DEBIT_USER debit;
        private Timer _timerFocus;
        private long? _operator_id;
        private double _session_id;

        #endregion


        public bool CloseQuery()
        {

            if (dal != null) dal.Close();

            return true;
        }

        public void Initilaize()
        {
            dal = new DataLayer();
            // System.Windows.Forms.MessageBox.Show("1");
            if (DEBUG)
            {
                dal.MockConnect();
                _operator_id = 1;
                _session_id = 1;
                //sdg = dal.FindBy<SDG_USER>(x => x.SDG_ID == 266).SingleOrDefault();


            }
            else
            {
                dal.Connect(_ntlsCon);
                _operator_id = (long)_ntlsUser.GetOperatorId();
                _session_id = _ntlsCon.GetSessionId();
            }
            // var customers = dal.GetAll<U_CUSTOMER>().ToList();


            Parts =
                dal.FindBy<U_PARTS>(p => p.NAME!=null)
                   .ToList();
            cmbParts.ItemsSource = Parts;

            RotherStatus = dal.GetPhraseEntries("Debit Status").ToList();
            cmbStatus.ItemsSource = RotherStatus;
            cmbStatus.SelectedValue = "N";
            // parts.First().U_PARTS_ID;


        }



        private void TxtRotherName_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter && e.Key != Key.Return)
                {
                    sdg = null;
                    debit = null;
                    TxtPathoLabName.Text = "";
                    TxtPayingCustomer.Text = "";
                    Txtclient.Text = "";
                    return;
                }
                else if (TxtRotherName.Text == "")
                {
                    return;
                }
                else if (!TxtRotherName.Text.StartsWith("R"))
                {
                    System.Windows.Forms.MessageBox.Show("רשומות חיוב אחר מתחילות באות R");
                    return;
                }
                else
                {

                    debit = null;
                    sdg = null;
                    try
                    {
                        TxtRotherName.Text = TxtRotherName.Text.ToUpper();
                        debit =
                            dal.FindBy<U_DEBIT_USER>(ru => ru.U_DEBIT.NAME == TxtRotherName.Text).SingleOrDefault();
                    }
                    catch (Exception)
                    {


                    }
                    if (debit == null)
                    {
                        //txtOrderName.Text = "";
                        //fillDetails();
                        CustomMessageBox.Show("רשומת החיוב לא נמצאה, אנא נסו שנית.");
                        //txtOrderName.Focus();
                    }
                }
                if (debit != null)
                {
                    //load order

                    //if(order.U_CUSTOMER !=null)   cmbCustomers.SelectedValue = order.U_CUSTOMER;
                    //btnSwitch.IsEnabled = true;
                    GetRotherInfo();
                }

            }



            catch (Exception ex)
            {
                CustomMessageBox.Show("אירעה תקלה");
                Logger.WriteLogFile(ex);
            }
        }

        private void GetRotherInfo()
        {
            TxtRotherName.Text = debit.U_DEBIT.NAME;
            try
            {
                sdg = dal.FindBy<SDG_USER>(d => d.SDG.NAME == debit.U_SDG_NAME).Include(d => d.SDG)
                         .Include(d => d.U_ORDER.U_ORDER_USER.U_CUSTOMER1)
                         .Include(d => d.CLIENT)
                         .Include(d => d.CLIENT.CLIENT_USER)
                         .SingleOrDefault();
            }
            catch (Exception)
            {

                sdg = null;
                CustomMessageBox.Show("המקרה לא נמצא, אנא נסו שנית.");
                return;
            }
            TxtPathoLabName.Text = sdg.U_PATHOLAB_NUMBER;
            fillDetails();
            cmbParts.SelectedValue = debit.U_PARTS_ID;
            TxtPrice.Text = debit.U_PART_PRICE.ToString();
            TxtQuantity.Text = debit.U_QUANTITY.ToString();
            TxtLineAmount.Text = debit.U_LINE_AMOUNT.ToString();
            TxtRemarks.Text = debit.U_DEBIT.DESCRIPTION;
            cmbStatus.SelectedValue = debit.U_DEBIT_STATUS;
        }


        private void TxtPathoLabName_KeyDown(object sender, KeyEventArgs e)
        {


            try
            {
                if (e.Key != Key.Enter && e.Key != Key.Return)
                {
                    debit = null;
                    sdg = null;
                    TxtPayingCustomer.Text = "";
                    Txtclient.Text = "";
                    TxtRotherName.Text = "";
                    return;
                }
                else if (TxtPathoLabName.Text == "")
                {
                    debit = null;
                    TxtPayingCustomer.Text = "";
                    Txtclient.Text = "";
                    TxtRotherName.Text = "";
                    return;
                }
                else
                {

                    try
                    {
                        sdg = dal.FindBy<SDG_USER>(d => d.U_PATHOLAB_NUMBER == TxtPathoLabName.Text).SingleOrDefault();
                    }
                    catch (Exception)
                    {

                        sdg = null;
                    }
                    if (sdg == null)
                    {
                        //txtOrderName.Text = "";
                        //fillDetails();
                        CustomMessageBox.Show("המקרה לא נמצא, אנא נסו שנית.");
                        //txtOrderName.Focus();
                    }
                }
                if (sdg != null)
                {
                    //load order

                    //if(order.U_CUSTOMER !=null)   cmbCustomers.SelectedValue = order.U_CUSTOMER;
                    //btnSwitch.IsEnabled = true;
                    fillDetails();


                }

            }



            catch (Exception ex)
            {
                CustomMessageBox.Show("אירעה תקלה");
                Logger.WriteLogFile(ex);
            }
        }


        private void fillDetails()
        {

            TxtPayingCustomer.Text = "";
            Txtclient.Text = "";

            if (sdg != null)
            {


                if (sdg.U_ORDER.U_ORDER_USER.U_CUSTOMER1 != null)
                    TxtPayingCustomer.Text = sdg.U_ORDER.U_ORDER_USER.U_CUSTOMER1.NAME;
                if (sdg.CLIENT != null)
                {
                    Txtclient.Text += sdg.CLIENT.CLIENT_USER.U_FIRST_NAME + " " + sdg.CLIENT.CLIENT_USER.U_LAST_NAME;
                }

                //get the last sdg connected to the order in case of revision
                // sdg = dal.FindBy<SDG_USER>(du => du.U_ORDER_ID == order.U_ORDER_ID).OrderByDescending(du => du.SDG_ID).FirstOrDefault();

            }
        }

        private void cmbCustomers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void txtOrderName_TouchEnter(object sender, TouchEventArgs e)
        {

        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            TxtPathoLabName.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TxtPathoLabName.Focus();
        }

        private void txtOrderName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TxtPathoLabName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void TxtLineAmount_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void TxtPrice_KeyDown(object sender, KeyEventArgs e)
        {


            decimal price = 0;
            decimal quantity = 0;
            TxtLineAmount.Text = "";
            bool goodPrice = false;
            bool goodQ = false;
            if (TxtPrice.Text == "" || !(goodPrice = decimal.TryParse(TxtPrice.Text, out price)))
            {
                TxtPrice.BorderBrush = Brushes.Red;
            }
            else
            {
                TxtPrice.BorderBrush = Brushes.GreenYellow;
            }
            if (TxtQuantity.Text == "" || !(goodQ = decimal.TryParse(TxtQuantity.Text, out quantity)))
            {
                TxtQuantity.BorderBrush = Brushes.Red;
            }
            else
            {
                TxtQuantity.BorderBrush = Brushes.GreenYellow;
            }
            if (goodPrice && goodQ)
            {
                TxtLineAmount.Text = (price * quantity).ToString();
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            U_DEBIT debitP;
            long selectedPart;

            if (sdg == null)
            {
                CustomMessageBox.Show("נא להזין מספר פתולאב ולהקיש ENTER");
                return;
            }
            if (cmbParts.SelectedValue == null || !long.TryParse(cmbParts.SelectedValue.ToString(), out selectedPart))
            {
                System.Windows.Forms.MessageBox.Show("הפריט שנבחר אינו תקין");
                return;
            }
            U_PARTS part;
            try
            {
                part = dal.FindBy<U_PARTS>(p => p.U_PARTS_ID == selectedPart).Include(p => p.U_PARTS_USER).SingleOrDefault();

            }
            catch (Exception)
            {

                System.Windows.Forms.MessageBox.Show("הפריט שנבחר אינו תקין. לא ניתן לשלוף את הרשומה.");
                return;
            }
            decimal price;
            if (TxtPrice.Text == "" || !(decimal.TryParse(TxtPrice.Text, out  price)))
            {
                TxtPrice.BorderBrush = Brushes.Red;
                System.Windows.Forms.MessageBox.Show("המחיר שנבחר אינו תקין");
                return;
            }

            decimal quantity;
            if (TxtQuantity.Text == "" || !(decimal.TryParse(TxtQuantity.Text, out quantity)))
            {
                System.Windows.Forms.MessageBox.Show("הכמות שנבחרה אינה תקינה", "שגיאה");
                return;
            }
            if (cmbStatus.SelectedValue == null)
            {
                System.Windows.Forms.MessageBox.Show("לא נבחר סטטוס", "שגיאה");
                return;
            }

            decimal priceIncludingVAT;
            PHRASE_HEADER Params = dal.GetPhraseByName("System Parameters");
            string vatString;
            Params.PhraseEntriesDictonary.TryGetValue("Vat Precent", out vatString);
            vatString = vatString.Replace("%", "");
            decimal vat = Convert.ToDecimal(vatString);
            priceIncludingVAT = price * (vat / 100 + 1);
            if (debit == null)
            {
                //new debit
                long sequenceId = (long)dal.GetNewId("SQ_U_DEBIT");


                debitP = new U_DEBIT()
                {
                    U_DEBIT_ID = (long)sequenceId,
                    NAME = "R" + sequenceId.ToString(),
                    DESCRIPTION = TxtRemarks.Text,
                    VERSION = "1",
                    VERSION_STATUS = "A"
                };

                debitP.U_DEBIT_USER = new U_DEBIT_USER
                {
                    U_DEBIT_ID = (long)sequenceId,
                    U_SDG_NAME = sdg.SDG.NAME,
                    U_PARTS_ID = selectedPart,
                    U_QUANTITY = quantity,
                    U_PART_PRICE =  Round3(price),
                    U_PRICE_INC_VAT =  Round3(priceIncludingVAT),
                    U_LINE_AMOUNT =  Round3(quantity * priceIncludingVAT),
                    U_ENTITY_ID = sdg.SDG.NAME,
                    U_EVENT_DATE = dal.GetSysdate(),
                    U_DEBIT_STATUS = cmbStatus.SelectedValue.ToString(),
                    U_ORDER_ID = sdg.U_ORDER.U_ORDER_ID,
                    U_PART_TEXT = "חיובים אחרים:(" + _operator_id + ")" + part.DESCRIPTION,
                    U_LAST_UPDATE = (null),

                };
                dal.Add(debitP);
                dal.SaveChanges();
                dal.InsertToSdgLog(sdg.SDG_ID, "New Roter", (long)_session_id, debitP.NAME + "," +
                                                                                     cmbParts.SelectedValue.ToString()
                                                                                    + "," + (quantity * price).ToString());
                debit = debitP.U_DEBIT_USER;
                GetRotherInfo();
                System.Windows.Forms.MessageBox.Show("הרשומה נוצרה בהצלחה");
                BtnClean_Click(null, null);

            }
            else
            {
                debitP = debit.U_DEBIT;
                debitP.DESCRIPTION = TxtRemarks.Text;
                debit.U_PARTS_ID = selectedPart;
                debit.U_QUANTITY = quantity;
                debit.U_PART_PRICE =Round3( price);
                debit.U_PRICE_INC_VAT = Round3(priceIncludingVAT);
                debit.U_LINE_AMOUNT = Round3(quantity * priceIncludingVAT);
                debit.U_EVENT_DATE = dal.GetSysdate();
                debit.U_DEBIT_STATUS = cmbStatus.SelectedValue.ToString();
                dal.SaveChanges();
                dal.InsertToSdgLog(sdg.SDG_ID, "Edit Roter", (long)_session_id, debitP.NAME + "," +
                                                                                   cmbParts.SelectedValue.ToString()
                                                                                  + "," + (quantity * price).ToString());
                GetRotherInfo();
                System.Windows.Forms.MessageBox.Show("הרשומה עודכנה בהצלחה");
                BtnClean_Click(null, null);

            }
        }
        private decimal Round3(decimal? x)
        {

            return decimal.Round(x ?? 0, 3);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (!DEBUG) _ntlsSite.CloseWindow();
        }

        private void BtnClean_Click(object sender, RoutedEventArgs e)
        {
            sdg = null;
            debit = null;
            TxtRotherName.Text = "";
            TxtPathoLabName.Text = "";
            TxtPayingCustomer.Text = "";
            Txtclient.Text = "";
            cmbParts.SelectedValue = null;
            cmbStatus.SelectedValue = null;
            TxtPrice.Text = "";
            TxtQuantity.Text = "1";
            TxtRemarks.Text = "";
            TxtLineAmount.Text = "";


        }


    }
}