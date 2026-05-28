using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace proiect_poo
{
    public partial class FormEditor : Form
    {
        // Obiectele de date engine
        private StoryJsonDefinition _povesteCurenta;
        private string _caleFichierCurent = "";

        // Obiectele selectate curent pentru editare
        private BlockJsonDefinition _blocCurent;
        private DecisionJsonDefinition _decizieCurenta;
        private bool _seIncarcaDatele = false;

        // --- CONTROALE UI DECLARATE CONFORM STANDARDULUI DESIGNER ---
        private System.Windows.Forms.TextBox txtTitluPoveste;
        private System.Windows.Forms.Label lblTitluPoveste;
        private System.Windows.Forms.TreeView treeViewStructura;
        private System.Windows.Forms.Button btnCreazaNoua;
        private System.Windows.Forms.Button btnIncarcaExistenta;
        private System.Windows.Forms.Button btnSalveaza;
        private System.Windows.Forms.Button btnAdaugaZi;
        private System.Windows.Forms.Button btnAdaugaBloc;

        private System.Windows.Forms.ListBox lstStatusuri;
        private System.Windows.Forms.TextBox txtStatusNume;
        private System.Windows.Forms.Button btnAdaugaStatus;
        private System.Windows.Forms.Button btnStergeStatus;

        // Butoane noi pentru ordonare Statusuri și Blocuri
        private System.Windows.Forms.Button btnStatusSus;
        private System.Windows.Forms.Button btnStatusJos;
        private System.Windows.Forms.Button btnBlocSus;
        private System.Windows.Forms.Button btnBlocJos;

        private System.Windows.Forms.Panel panelEditareBloc;
        private System.Windows.Forms.TextBox txtBlockId;
        private System.Windows.Forms.TextBox txtBlockText;
        private System.Windows.Forms.ListBox lstDecizii;
        private System.Windows.Forms.Button btnAdaugaDecizie;
        private System.Windows.Forms.Button btnStergeDecizie;
        private System.Windows.Forms.TextBox txtDecizieText;
        private System.Windows.Forms.TextBox txtDecizieDestinatie;
        private System.Windows.Forms.TextBox txtDecizieEfecte;

        // Butoane noi pentru ordonare Decizii
        private System.Windows.Forms.Button btnDecizieSus;
        private System.Windows.Forms.Button btnDecizieJos;

        // Etichetele text fixe
        private System.Windows.Forms.Label lblStatusuriTitlu;
        private System.Windows.Forms.Label lblStructuraTitlu;
        private System.Windows.Forms.Label lblEditareBlocTitlu;
        private System.Windows.Forms.Label lblBlockIdTitlu;
        private System.Windows.Forms.Label lblBlockTextTitlu;
        private System.Windows.Forms.Label lblDeciziiTitlu;
        private System.Windows.Forms.Label lblDecizieTextTitlu;
        private System.Windows.Forms.Label lblDecizieDestinatieTitlu;
        private System.Windows.Forms.Label lblDecizieEfecteTitlu;

        public FormEditor()
        {
            InitializeComponent();
            ConfiguratieInitialaUI();
        }

        private void ConfiguratieInitialaUI()
        {
            SetareStareEditare(false);
            panelEditareBloc.Visible = false;
        }

        private void SetareStareEditare(bool activa)
        {
            txtTitluPoveste.Enabled = activa;
            lstStatusuri.Enabled = activa;
            txtStatusNume.Enabled = activa;
            btnAdaugaStatus.Enabled = activa;
            btnStergeStatus.Enabled = activa;
            treeViewStructura.Enabled = activa;
            btnAdaugaZi.Enabled = activa;
            btnAdaugaBloc.Enabled = activa;
            btnSalveaza.Enabled = activa;

            // Activare butoane ordonare globale
            btnStatusSus.Enabled = activa;
            btnStatusJos.Enabled = activa;
            btnBlocSus.Enabled = activa;
            btnBlocJos.Enabled = activa;
        }

        // =====================================================================
        // EVENIMENTE JSON & WORKSPACE
        // =====================================================================

        private void btnCreazaNoua_Click(object sender, EventArgs e)
        {
            _povesteCurenta = new StoryJsonDefinition
            {
                Title = "Poveste Nouă",
                StartBlock = "start_1",
                Properties = new List<PropertyJsonDefinition>(),
                Ideas = new List<IdeaJsonDefinition>(),
                Days = new List<DayJsonDefinition>()
            };

            _povesteCurenta.Properties.Add(new PropertyJsonDefinition { Key = "stres", HudLabel = "Stres", Min = 0, Max = 100, Initial = 25, VisibleInHud = true });

            var primaZi = new DayJsonDefinition { Name = "Ziua 1", Blocks = new List<BlockJsonDefinition>() };
            var primulBloc = new BlockJsonDefinition
            {
                Id = "start_1",
                Text = "Scrie textul de început aici...",
                BlockType = "normal",
                Decisions = new List<DecisionJsonDefinition>()
            };
            primaZi.Blocks.Add(primulBloc);
            _povesteCurenta.Days.Add(primaZi);

            _caleFichierCurent = "";
            AfiseazaWorkspace();
        }

        private void btnIncarcaExistenta_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fișiere JSON (*.json)|*.json";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _povesteCurenta = JsonManager.IncarcaPoveste(ofd.FileName);
                        _caleFichierCurent = ofd.FileName;
                        AfiseazaWorkspace();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la deschiderea fișierului: " + ex.Message);
                    }
                }
            }
        }

        private void AfiseazaWorkspace()
        {
            SetareStareEditare(true);
            panelEditareBloc.Visible = false;
            _blocCurent = null;

            txtTitluPoveste.Text = _povesteCurenta.Title;
            ActualizeazaTreeView();
            ActualizeazaListaStatusuri();
        }

        // =====================================================================
        // REFRESH-URI INTERFAȚĂ
        // =====================================================================

        private void ActualizeazaTreeView()
        {
            treeViewStructura.Nodes.Clear();
            if (_povesteCurenta.Days == null) return;

            foreach (var zi in _povesteCurenta.Days)
            {
                TreeNode nodZi = new TreeNode(zi.Name);
                nodZi.Tag = zi;

                if (zi.Blocks != null)
                {
                    foreach (var bloc in zi.Blocks)
                    {
                        TreeNode nodBloc = new TreeNode($"[{bloc.Id}] {SubsirText(bloc.Text, 15)}");
                        nodBloc.Tag = bloc;
                        nodZi.Nodes.Add(nodBloc);
                    }
                }
                treeViewStructura.Nodes.Add(nodZi);
            }
            treeViewStructura.ExpandAll();
        }

        private void ActualizeazaListaStatusuri()
        {
            lstStatusuri.Items.Clear();
            if (_povesteCurenta.Properties == null) return;

            foreach (var prop in _povesteCurenta.Properties)
            {
                lstStatusuri.Items.Add(prop.Key);
            }
        }

        private void ActualizeazaListaDecizii()
        {
            lstDecizii.Items.Clear();
            if (_blocCurent?.Decisions == null) return;

            for (int i = 0; i < _blocCurent.Decisions.Count; i++)
            {
                var dec = _blocCurent.Decisions[i];
                lstDecizii.Items.Add($"Opț. {i + 1}: {SubsirText(dec.Text, 15)} -> [{dec.TargetBlock}]");
            }
        }

        private void SelecteazaNodDupaTag(object tag)
        {
            if (tag == null) return;
            foreach (TreeNode nodZi in treeViewStructura.Nodes)
            {
                if (nodZi.Tag == tag) { treeViewStructura.SelectedNode = nodZi; return; }
                foreach (TreeNode nodBloc in nodZi.Nodes)
                {
                    if (nodBloc.Tag == tag) { treeViewStructura.SelectedNode = nodBloc; return; }
                }
            }
        }

        // =====================================================================
        // LOGICA DE SELECTARE
        // =====================================================================

        private void treeViewStructura_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is BlockJsonDefinition bloc)
            {
                _blocCurent = bloc;
                _decizieCurenta = null;

                _seIncarcaDatele = true;

                panelEditareBloc.Visible = true;
                txtBlockId.Text = bloc.Id;
                txtBlockText.Text = bloc.Text;

                ActualizeazaListaDecizii();
                SetareStareEditareDecizie(false);

                _seIncarcaDatele = false;
            }
            else
            {
                _blocCurent = null;
                panelEditareBloc.Visible = false;
            }
        }

        private void lstDecizii_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstDecizii.SelectedIndex;
            if (idx >= 0 && _blocCurent?.Decisions != null && idx < _blocCurent.Decisions.Count)
            {
                _decizieCurenta = _blocCurent.Decisions[idx];

                _seIncarcaDatele = true;
                txtDecizieText.Text = _decizieCurenta.Text;
                txtDecizieDestinatie.Text = _decizieCurenta.TargetBlock;

                if (_decizieCurenta.Effects != null)
                {
                    txtDecizieEfecte.Text = JsonConvert.SerializeObject(_decizieCurenta.Effects, Newtonsoft.Json.Formatting.Indented);
                }
                else
                {
                    txtDecizieEfecte.Text = "[]";
                }

                _seIncarcaDatele = false;
                SetareStareEditareDecizie(true);
            }
            else
            {
                _decizieCurenta = null;
                SetareStareEditareDecizie(false);
            }
        }

        private void SetareStareEditareDecizie(bool activa)
        {
            txtDecizieText.Enabled = activa;
            txtDecizieDestinatie.Enabled = activa;
            txtDecizieEfecte.Enabled = activa;
            btnDecizieSus.Enabled = activa;
            btnDecizieJos.Enabled = activa;
        }

        // =====================================================================
        // MODIFICĂRI LIVE
        // =====================================================================

        private void txtBlockId_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.Id = txtBlockId.Text;

            if (treeViewStructura.SelectedNode != null)
                treeViewStructura.SelectedNode.Text = $"[{_blocCurent.Id}] {SubsirText(_blocCurent.Text, 15)}";
        }

        private void txtBlockText_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _blocCurent == null) return;
            _blocCurent.Text = txtBlockText.Text;

            if (treeViewStructura.SelectedNode != null)
                treeViewStructura.SelectedNode.Text = $"[{_blocCurent.Id}] {SubsirText(_blocCurent.Text, 15)}";
        }

        private void txtDecizieText_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;
            _decizieCurenta.Text = txtDecizieText.Text;

            _seIncarcaDatele = true;
            ActualizeazaListaDecizii();
            _seIncarcaDatele = false;
        }

        private void txtDecizieDestinatie_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;
            _decizieCurenta.TargetBlock = txtDecizieDestinatie.Text;

            _seIncarcaDatele = true;
            ActualizeazaListaDecizii();
            _seIncarcaDatele = false;
        }

        private void txtDecizieEfecte_TextChanged(object sender, EventArgs e)
        {
            if (_seIncarcaDatele || _decizieCurenta == null) return;

            try
            {
                var efecteSalvate = JsonConvert.DeserializeObject<List<EffectJsonDefinition>>(txtDecizieEfecte.Text);
                if (efecteSalvate != null)
                {
                    _decizieCurenta.Effects = efecteSalvate;
                }
            }
            catch
            {
                // Ignorăm erorile temporare de sintaxă în timpul tastării
            }
        }

        // =====================================================================
        // LOGICA DE REORDONARE (SUS / JOS)
        // =====================================================================

        private void btnStatusSus_Click(object sender, EventArgs e)
        {
            int idx = lstStatusuri.SelectedIndex;
            if (idx > 0 && _povesteCurenta?.Properties != null)
            {
                var prop = _povesteCurenta.Properties[idx];
                _povesteCurenta.Properties.RemoveAt(idx);
                _povesteCurenta.Properties.Insert(idx - 1, prop);
                ActualizeazaListaStatusuri();
                lstStatusuri.SelectedIndex = idx - 1;
            }
        }

        private void btnStatusJos_Click(object sender, EventArgs e)
        {
            int idx = lstStatusuri.SelectedIndex;
            if (idx >= 0 && idx < lstStatusuri.Items.Count - 1 && _povesteCurenta?.Properties != null)
            {
                var prop = _povesteCurenta.Properties[idx];
                _povesteCurenta.Properties.RemoveAt(idx);
                _povesteCurenta.Properties.Insert(idx + 1, prop);
                ActualizeazaListaStatusuri();
                lstStatusuri.SelectedIndex = idx + 1;
            }
        }

        private void btnBlocSus_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectat = treeViewStructura.SelectedNode;
            if (nodSelectat == null || !(nodSelectat.Tag is BlockJsonDefinition bloc)) return;
            TreeNode nodParinte = nodSelectat.Parent;
            if (nodParinte == null || !(nodParinte.Tag is DayJsonDefinition zi)) return;

            int idx = zi.Blocks.IndexOf(bloc);
            if (idx > 0)
            {
                zi.Blocks.RemoveAt(idx);
                zi.Blocks.Insert(idx - 1, bloc);
                ActualizeazaTreeView();
                SelecteazaNodDupaTag(bloc);
            }
        }

        private void btnBlocJos_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectat = treeViewStructura.SelectedNode;
            if (nodSelectat == null || !(nodSelectat.Tag is BlockJsonDefinition bloc)) return;
            TreeNode nodParinte = nodSelectat.Parent;
            if (nodParinte == null || !(nodParinte.Tag is DayJsonDefinition zi)) return;

            int idx = zi.Blocks.IndexOf(bloc);
            if (idx >= 0 && idx < zi.Blocks.Count - 1)
            {
                zi.Blocks.RemoveAt(idx);
                zi.Blocks.Insert(idx + 1, bloc);
                ActualizeazaTreeView();
                SelecteazaNodDupaTag(bloc);
            }
        }

        private void btnDecizieSus_Click(object sender, EventArgs e)
        {
            int idx = lstDecizii.SelectedIndex;
            if (idx > 0 && _blocCurent?.Decisions != null)
            {
                var dec = _blocCurent.Decisions[idx];
                _blocCurent.Decisions.RemoveAt(idx);
                _blocCurent.Decisions.Insert(idx - 1, dec);
                ActualizeazaListaDecizii();
                lstDecizii.SelectedIndex = idx - 1;
            }
        }

        private void btnDecizieJos_Click(object sender, EventArgs e)
        {
            int idx = lstDecizii.SelectedIndex;
            if (idx >= 0 && idx < lstDecizii.Items.Count - 1 && _blocCurent?.Decisions != null)
            {
                var dec = _blocCurent.Decisions[idx];
                _blocCurent.Decisions.RemoveAt(idx);
                _blocCurent.Decisions.Insert(idx + 1, dec);
                ActualizeazaListaDecizii();
                lstDecizii.SelectedIndex = idx + 1;
            }
        }

        // =====================================================================
        // ADAUGĂRI / ȘTERGERI DE SUB-ELEMENTE
        // =====================================================================

        private void btnAdaugaStatus_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStatusNume.Text)) return;
            if (_povesteCurenta.Properties == null) _povesteCurenta.Properties = new List<PropertyJsonDefinition>();

            _povesteCurenta.Properties.Add(new PropertyJsonDefinition
            {
                Key = txtStatusNume.Text,
                HudLabel = txtStatusNume.Text,
                Min = 0,
                Max = 100,
                Initial = 0,
                VisibleInHud = true
            });

            txtStatusNume.Clear();
            ActualizeazaListaStatusuri();
        }

        private void btnStergeStatus_Click(object sender, EventArgs e)
        {
            int idx = lstStatusuri.SelectedIndex;
            if (idx >= 0)
            {
                _povesteCurenta.Properties.RemoveAt(idx);
                ActualizeazaListaStatusuri();
            }
        }

        private void btnAdaugaZi_Click(object sender, EventArgs e)
        {
            int numarZile = _povesteCurenta.Days.Count + 1;
            _povesteCurenta.Days.Add(new DayJsonDefinition { Id = "zi" + numarZile, Name = "Ziua " + numarZile, Blocks = new List<BlockJsonDefinition>() });
            ActualizeazaTreeView();
        }

        private void btnAdaugaBloc_Click(object sender, EventArgs e)
        {
            TreeNode nodSelectat = treeViewStructura.SelectedNode;
            DayJsonDefinition ziTinta = null;
            if (nodSelectat?.Tag is DayJsonDefinition) ziTinta = (DayJsonDefinition)nodSelectat.Tag;
            else if (nodSelectat?.Parent?.Tag is DayJsonDefinition) ziTinta = (DayJsonDefinition)nodSelectat.Parent.Tag;

            if (ziTinta == null) { MessageBox.Show("Selectează o Zi din listă!"); return; }

            ziTinta.Blocks.Add(new BlockJsonDefinition
            {
                Id = "block_nou_" + Guid.NewGuid().ToString().Substring(0, 4),
                Text = "Text poveste nou...",
                BlockType = "normal",
                Decisions = new List<DecisionJsonDefinition>()
            });
            ActualizeazaTreeView();
        }

        private void btnAdaugaDecizie_Click(object sender, EventArgs e)
        {
            if (_blocCurent == null) return;
            if (_blocCurent.Decisions == null) _blocCurent.Decisions = new List<DecisionJsonDefinition>();

            _blocCurent.Decisions.Add(new DecisionJsonDefinition { Text = "Opțiune nouă...", TargetBlock = "block_z1_dimineata", Effects = new List<EffectJsonDefinition>() });
            ActualizeazaListaDecizii();
        }

        private void btnStergeDecizie_Click(object sender, EventArgs e)
        {
            int idx = lstDecizii.SelectedIndex;
            if (idx >= 0 && _blocCurent?.Decisions != null)
            {
                _blocCurent.Decisions.RemoveAt(idx);
                _decizieCurenta = null;
                ActualizeazaListaDecizii();
                SetareStareEditareDecizie(false);
            }
        }

        private void btnSalveaza_Click(object sender, EventArgs e)
        {
            _povesteCurenta.Title = txtTitluPoveste.Text;
            if (string.IsNullOrEmpty(_caleFichierCurent))
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "Fișiere JSON (*.json)|*.json";
                    if (sfd.ShowDialog() == DialogResult.OK) _caleFichierCurent = sfd.FileName;
                    else return;
                }
            }
            try
            {
                JsonManager.SalveazaPoveste(_caleFichierCurent, _povesteCurenta);
                MessageBox.Show("Povestea a fost salvată cu succes!");
            }
            catch (Exception ex) { MessageBox.Show("Eroare la salvare: " + ex.Message); }
        }

        private string SubsirText(string text, int lungimeMax)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text.Length <= lungimeMax ? text : text.Substring(0, lungimeMax) + "...";
        }

        // =====================================================================
        // CONSTRUCTOR INITIALIZE COMPONENT - FORMAT CLASIC COMPATIBIL DESIGNER
        // =====================================================================
        private void InitializeComponent()
        {
            this.txtTitluPoveste = new System.Windows.Forms.TextBox();
            this.lblTitluPoveste = new System.Windows.Forms.Label();
            this.treeViewStructura = new System.Windows.Forms.TreeView();
            this.btnCreazaNoua = new System.Windows.Forms.Button();
            this.btnIncarcaExistenta = new System.Windows.Forms.Button();
            this.btnSalveaza = new System.Windows.Forms.Button();
            this.btnAdaugaZi = new System.Windows.Forms.Button();
            this.btnAdaugaBloc = new System.Windows.Forms.Button();
            this.lstStatusuri = new System.Windows.Forms.ListBox();
            this.txtStatusNume = new System.Windows.Forms.TextBox();
            this.btnAdaugaStatus = new System.Windows.Forms.Button();
            this.btnStergeStatus = new System.Windows.Forms.Button();

            // Instanțiere butoane reordonare stânga
            this.btnStatusSus = new System.Windows.Forms.Button();
            this.btnStatusJos = new System.Windows.Forms.Button();
            this.btnBlocSus = new System.Windows.Forms.Button();
            this.btnBlocJos = new System.Windows.Forms.Button();

            this.panelEditareBloc = new System.Windows.Forms.Panel();
            this.txtBlockId = new System.Windows.Forms.TextBox();
            this.txtBlockText = new System.Windows.Forms.TextBox();
            this.lstDecizii = new System.Windows.Forms.ListBox();
            this.btnAdaugaDecizie = new System.Windows.Forms.Button();
            this.btnStergeDecizie = new System.Windows.Forms.Button();
            this.txtDecizieText = new System.Windows.Forms.TextBox();
            this.txtDecizieDestinatie = new System.Windows.Forms.TextBox();
            this.txtDecizieEfecte = new System.Windows.Forms.TextBox();

            // Instanțiere butoane reordonare dreapta
            this.btnDecizieSus = new System.Windows.Forms.Button();
            this.btnDecizieJos = new System.Windows.Forms.Button();

            this.lblStatusuriTitlu = new System.Windows.Forms.Label();
            this.lblStructuraTitlu = new System.Windows.Forms.Label();
            this.lblEditareBlocTitlu = new System.Windows.Forms.Label();
            this.lblBlockIdTitlu = new System.Windows.Forms.Label();
            this.lblBlockTextTitlu = new System.Windows.Forms.Label();
            this.lblDeciziiTitlu = new System.Windows.Forms.Label();
            this.lblDecizieTextTitlu = new System.Windows.Forms.Label();
            this.lblDecizieDestinatieTitlu = new System.Windows.Forms.Label();
            this.lblDecizieEfecteTitlu = new System.Windows.Forms.Label();

            this.panelEditareBloc.SuspendLayout();
            this.SuspendLayout();

            // btnCreazaNoua
            this.btnCreazaNoua.Location = new System.Drawing.Point(12, 12);
            this.btnCreazaNoua.Name = "btnCreazaNoua";
            this.btnCreazaNoua.Size = new System.Drawing.Size(130, 30);
            this.btnCreazaNoua.Text = "➕ Poveste Nouă";
            this.btnCreazaNoua.UseVisualStyleBackColor = true;
            this.btnCreazaNoua.Click += new System.EventHandler(this.btnCreazaNoua_Click);

            // btnIncarcaExistenta
            this.btnIncarcaExistenta.Location = new System.Drawing.Point(148, 12);
            this.btnIncarcaExistenta.Name = "btnIncarcaExistenta";
            this.btnIncarcaExistenta.Size = new System.Drawing.Size(130, 30);
            this.btnIncarcaExistenta.Text = "📂 Deschide JSON";
            this.btnIncarcaExistenta.UseVisualStyleBackColor = true;
            this.btnIncarcaExistenta.Click += new System.EventHandler(this.btnIncarcaExistenta_Click);

            // btnSalveaza
            this.btnSalveaza.Location = new System.Drawing.Point(908, 12);
            this.btnSalveaza.Name = "btnSalveaza";
            this.btnSalveaza.Size = new System.Drawing.Size(130, 30);
            this.btnSalveaza.Text = "💾 Salvează";
            this.btnSalveaza.UseVisualStyleBackColor = true;
            this.btnSalveaza.Click += new System.EventHandler(this.btnSalveaza_Click);

            // lblTitluPoveste
            this.lblTitluPoveste.Location = new System.Drawing.Point(300, 17);
            this.lblTitluPoveste.Name = "lblTitluPoveste";
            this.lblTitluPoveste.Size = new System.Drawing.Size(80, 20);
            this.lblTitluPoveste.Text = "Titlu Proiect:";

            // txtTitluPoveste
            this.txtTitluPoveste.Location = new System.Drawing.Point(380, 14);
            this.txtTitluPoveste.Name = "txtTitluPoveste";
            this.txtTitluPoveste.Size = new System.Drawing.Size(300, 20);

            // lblStatusuriTitlu
            this.lblStatusuriTitlu.Location = new System.Drawing.Point(12, 55);
            this.lblStatusuriTitlu.Name = "lblStatusuriTitlu";
            this.lblStatusuriTitlu.Size = new System.Drawing.Size(266, 18);
            this.lblStatusuriTitlu.Text = "📊 Statusuri Globale (Properties):";

            // lstStatusuri (Lățime redusă ușor pentru a face loc butoanelor)
            this.lstStatusuri.Location = new System.Drawing.Point(12, 75);
            this.lstStatusuri.Name = "lstStatusuri";
            this.lstStatusuri.Size = new System.Drawing.Size(220, 82);

            // btnStatusSus
            this.btnStatusSus.Location = new System.Drawing.Point(238, 75);
            this.btnStatusSus.Name = "btnStatusSus";
            this.btnStatusSus.Size = new System.Drawing.Size(40, 40);
            this.btnStatusSus.Text = "▲";
            this.btnStatusSus.UseVisualStyleBackColor = true;
            this.btnStatusSus.Click += new System.EventHandler(this.btnStatusSus_Click);

            // btnStatusJos
            this.btnStatusJos.Location = new System.Drawing.Point(238, 117);
            this.btnStatusJos.Name = "btnStatusJos";
            this.btnStatusJos.Size = new System.Drawing.Size(40, 40);
            this.btnStatusJos.Text = "▼";
            this.btnStatusJos.UseVisualStyleBackColor = true;
            this.btnStatusJos.Click += new System.EventHandler(this.btnStatusJos_Click);

            // txtStatusNume
            this.txtStatusNume.Location = new System.Drawing.Point(12, 162);
            this.txtStatusNume.Name = "txtStatusNume";
            this.txtStatusNume.Size = new System.Drawing.Size(136, 20);

            // btnAdaugaStatus
            this.btnAdaugaStatus.Location = new System.Drawing.Point(154, 160);
            this.btnAdaugaStatus.Name = "btnAdaugaStatus";
            this.btnAdaugaStatus.Size = new System.Drawing.Size(60, 23);
            this.btnAdaugaStatus.Text = "➕ Add";
            this.btnAdaugaStatus.UseVisualStyleBackColor = true;
            this.btnAdaugaStatus.Click += new System.EventHandler(this.btnAdaugaStatus_Click);

            // btnStergeStatus
            this.btnStergeStatus.Location = new System.Drawing.Point(218, 160);
            this.btnStergeStatus.Name = "btnStergeStatus";
            this.btnStergeStatus.Size = new System.Drawing.Size(60, 23);
            this.btnStergeStatus.Text = "❌ Del";
            this.btnStergeStatus.UseVisualStyleBackColor = true;
            this.btnStergeStatus.Click += new System.EventHandler(this.btnStergeStatus_Click);

            // lblStructuraTitlu
            this.lblStructuraTitlu.Location = new System.Drawing.Point(12, 195);
            this.lblStructuraTitlu.Name = "lblStructuraTitlu";
            this.lblStructuraTitlu.Size = new System.Drawing.Size(266, 18);
            this.lblStructuraTitlu.Text = "🌲 Structură Zile & Blocuri:";

            // treeViewStructura (Lățime redusă pentru loc butoane)
            this.treeViewStructura.Location = new System.Drawing.Point(12, 215);
            this.treeViewStructura.Name = "treeViewStructura";
            this.treeViewStructura.Size = new System.Drawing.Size(220, 350);
            this.treeViewStructura.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewStructura_AfterSelect);

            // btnBlocSus
            this.btnBlocSus.Location = new System.Drawing.Point(238, 215);
            this.btnBlocSus.Name = "btnBlocSus";
            this.btnBlocSus.Size = new System.Drawing.Size(40, 35);
            this.btnBlocSus.Text = "▲";
            this.btnBlocSus.UseVisualStyleBackColor = true;
            this.btnBlocSus.Click += new System.EventHandler(this.btnBlocSus_Click);

            // btnBlocJos
            this.btnBlocJos.Location = new System.Drawing.Point(238, 255);
            this.btnBlocJos.Name = "btnBlocJos";
            this.btnBlocJos.Size = new System.Drawing.Size(40, 35);
            this.btnBlocJos.Text = "▼";
            this.btnBlocJos.UseVisualStyleBackColor = true;
            this.btnBlocJos.Click += new System.EventHandler(this.btnBlocJos_Click);

            // btnAdaugaZi
            this.btnAdaugaZi.Location = new System.Drawing.Point(12, 575);
            this.btnAdaugaZi.Name = "btnAdaugaZi";
            this.btnAdaugaZi.Size = new System.Drawing.Size(130, 30);
            this.btnAdaugaZi.Text = "📅 Adaugă Zi";
            this.btnAdaugaZi.UseVisualStyleBackColor = true;
            this.btnAdaugaZi.Click += new System.EventHandler(this.btnAdaugaZi_Click);

            // btnAdaugaBloc
            this.btnAdaugaBloc.Location = new System.Drawing.Point(148, 575);
            this.btnAdaugaBloc.Name = "btnAdaugaBloc";
            this.btnAdaugaBloc.Size = new System.Drawing.Size(130, 30);
            this.btnAdaugaBloc.Text = "📄 Adaugă Bloc";
            this.btnAdaugaBloc.UseVisualStyleBackColor = true;
            this.btnAdaugaBloc.Click += new System.EventHandler(this.btnAdaugaBloc_Click);

            // panelEditareBloc
            this.panelEditareBloc.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelEditareBloc.Controls.Add(this.lblEditareBlocTitlu);
            this.panelEditareBloc.Controls.Add(this.lblBlockIdTitlu);
            this.panelEditareBloc.Controls.Add(this.txtBlockId);
            this.panelEditareBloc.Controls.Add(this.lblBlockTextTitlu);
            this.panelEditareBloc.Controls.Add(this.txtBlockText);
            this.panelEditareBloc.Controls.Add(this.lblDeciziiTitlu);
            this.panelEditareBloc.Controls.Add(this.lstDecizii);
            this.panelEditareBloc.Controls.Add(this.btnDecizieSus);
            this.panelEditareBloc.Controls.Add(this.btnDecizieJos);
            this.panelEditareBloc.Controls.Add(this.btnAdaugaDecizie);
            this.panelEditareBloc.Controls.Add(this.btnStergeDecizie);
            this.panelEditareBloc.Controls.Add(this.lblDecizieTextTitlu);
            this.panelEditareBloc.Controls.Add(this.txtDecizieText);
            this.panelEditareBloc.Controls.Add(this.lblDecizieDestinatieTitlu);
            this.panelEditareBloc.Controls.Add(this.txtDecizieDestinatie);
            this.panelEditareBloc.Controls.Add(this.lblDecizieEfecteTitlu);
            this.panelEditareBloc.Controls.Add(this.txtDecizieEfecte);
            this.panelEditareBloc.Location = new System.Drawing.Point(300, 55);
            this.panelEditareBloc.Name = "panelEditareBloc";
            this.panelEditareBloc.Size = new System.Drawing.Size(738, 550);

            // lblEditareBlocTitlu
            this.lblEditareBlocTitlu.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lblEditareBlocTitlu.Location = new System.Drawing.Point(10, 10);
            this.lblEditareBlocTitlu.Name = "lblEditareBlocTitlu";
            this.lblEditareBlocTitlu.Size = new System.Drawing.Size(300, 20);
            this.lblEditareBlocTitlu.Text = "📝 EDITARE BLOC SELECTAT";

            // lblBlockIdTitlu
            this.lblBlockIdTitlu.Location = new System.Drawing.Point(10, 43);
            this.lblBlockIdTitlu.Name = "lblBlockIdTitlu";
            this.lblBlockIdTitlu.Size = new System.Drawing.Size(60, 20);
            this.lblBlockIdTitlu.Text = "ID Bloc:";

            // txtBlockId
            this.txtBlockId.Location = new System.Drawing.Point(80, 40);
            this.txtBlockId.Name = "txtBlockId";
            this.txtBlockId.Size = new System.Drawing.Size(220, 20);
            this.txtBlockId.TextChanged += new System.EventHandler(this.txtBlockId_TextChanged);

            // lblBlockTextTitlu
            this.lblBlockTextTitlu.Location = new System.Drawing.Point(10, 75);
            this.lblBlockTextTitlu.Name = "lblBlockTextTitlu";
            this.lblBlockTextTitlu.Size = new System.Drawing.Size(300, 15);
            this.lblBlockTextTitlu.Text = "Text poveste:";

            // txtBlockText
            this.txtBlockText.Location = new System.Drawing.Point(10, 95);
            this.txtBlockText.Multiline = true;
            this.txtBlockText.Name = "txtBlockText";
            this.txtBlockText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtBlockText.Size = new System.Drawing.Size(715, 120);
            this.txtBlockText.TextChanged += new System.EventHandler(this.txtBlockText_TextChanged);

            // lblDeciziiTitlu
            this.lblDeciziiTitlu.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
            this.lblDeciziiTitlu.Location = new System.Drawing.Point(10, 230);
            this.lblDeciziiTitlu.Name = "lblDeciziiTitlu";
            this.lblDeciziiTitlu.Size = new System.Drawing.Size(300, 20);
            this.lblDeciziiTitlu.Text = "🔘 Opțiuni / Decizii Jucător:";

            // lstDecizii (Lățime ajustată)
            this.lstDecizii.Location = new System.Drawing.Point(10, 255);
            this.lstDecizii.Name = "lstDecizii";
            this.lstDecizii.Size = new System.Drawing.Size(200, 108);
            this.lstDecizii.SelectedIndexChanged += new System.EventHandler(this.lstDecizii_SelectedIndexChanged);

            // btnDecizieSus
            this.btnDecizieSus.Location = new System.Drawing.Point(215, 255);
            this.btnDecizieSus.Name = "btnDecizieSus";
            this.btnDecizieSus.Size = new System.Drawing.Size(45, 50);
            this.btnDecizieSus.Text = "▲";
            this.btnDecizieSus.UseVisualStyleBackColor = true;
            this.btnDecizieSus.Click += new System.EventHandler(this.btnDecizieSus_Click);

            // btnDecizieJos
            this.btnDecizieJos.Location = new System.Drawing.Point(215, 310);
            this.btnDecizieJos.Name = "btnDecizieJos";
            this.btnDecizieJos.Size = new System.Drawing.Size(45, 53);
            this.btnDecizieJos.Text = "▼";
            this.btnDecizieJos.UseVisualStyleBackColor = true;
            this.btnDecizieJos.Click += new System.EventHandler(this.btnDecizieJos_Click);

            // btnAdaugaDecizie
            this.btnAdaugaDecizie.Location = new System.Drawing.Point(10, 372);
            this.btnAdaugaDecizie.Name = "btnAdaugaDecizie";
            this.btnAdaugaDecizie.Size = new System.Drawing.Size(120, 25);
            this.btnAdaugaDecizie.Text = "➕ Adaugă Opțiune";
            this.btnAdaugaDecizie.UseVisualStyleBackColor = true;
            this.btnAdaugaDecizie.Click += new System.EventHandler(this.btnAdaugaDecizie_Click);

            // btnStergeDecizie
            this.btnStergeDecizie.Location = new System.Drawing.Point(140, 372);
            this.btnStergeDecizie.Name = "btnStergeDecizie";
            this.btnStergeDecizie.Size = new System.Drawing.Size(120, 25);
            this.btnStergeDecizie.Text = "❌ Șterge Opțiune";
            this.btnStergeDecizie.UseVisualStyleBackColor = true;
            this.btnStergeDecizie.Click += new System.EventHandler(this.btnStergeDecizie_Click);

            // lblDecizieTextTitlu
            this.lblDecizieTextTitlu.Location = new System.Drawing.Point(280, 258);
            this.lblDecizieTextTitlu.Name = "lblDecizieTextTitlu";
            this.lblDecizieTextTitlu.Size = new System.Drawing.Size(90, 20);
            this.lblDecizieTextTitlu.Text = "Text Opțiune:";

            // txtDecizieText
            this.txtDecizieText.Location = new System.Drawing.Point(375, 255);
            this.txtDecizieText.Name = "txtDecizieText";
            this.txtDecizieText.Size = new System.Drawing.Size(350, 20);
            this.txtDecizieText.TextChanged += new System.EventHandler(this.txtDecizieText_TextChanged);

            // lblDecizieDestinatieTitlu
            this.lblDecizieDestinatieTitlu.Location = new System.Drawing.Point(280, 288);
            this.lblDecizieDestinatieTitlu.Name = "lblDecizieDestinatieTitlu";
            this.lblDecizieDestinatieTitlu.Size = new System.Drawing.Size(95, 20);
            this.lblDecizieDestinatieTitlu.Text = "Sari la Bloc (ID):";

            // txtDecizieDestinatie
            this.txtDecizieDestinatie.Location = new System.Drawing.Point(375, 285);
            this.txtDecizieDestinatie.Name = "txtDecizieDestinatie";
            this.txtDecizieDestinatie.Size = new System.Drawing.Size(350, 20);
            this.txtDecizieDestinatie.TextChanged += new System.EventHandler(this.txtDecizieDestinatie_TextChanged);

            // lblDecizieEfecteTitlu
            this.lblDecizieEfecteTitlu.Location = new System.Drawing.Point(280, 320);
            this.lblDecizieEfecteTitlu.Name = "lblDecizieEfecteTitlu";
            this.lblDecizieEfecteTitlu.Size = new System.Drawing.Size(350, 15);
            this.lblDecizieEfecteTitlu.Text = "Efecte Decizie (Stări, Idei, Research ca text JSON):";

            // txtDecizieEfecte
            this.txtDecizieEfecte.Font = new System.Drawing.Font("Consolas", 9F);
            this.txtDecizieEfecte.Location = new System.Drawing.Point(280, 340);
            this.txtDecizieEfecte.Multiline = true;
            this.txtDecizieEfecte.Name = "txtDecizieEfecte";
            this.txtDecizieEfecte.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDecizieEfecte.Size = new System.Drawing.Size(445, 180);
            this.txtDecizieEfecte.TextChanged += new System.EventHandler(this.txtDecizieEfecte_TextChanged);

            // FormEditor Configuration
            this.ClientSize = new System.Drawing.Size(1050, 620);
            this.Controls.Add(this.btnCreazaNoua);
            this.Controls.Add(this.btnIncarcaExistenta);
            this.Controls.Add(this.btnSalveaza);
            this.Controls.Add(this.lblTitluPoveste);
            this.Controls.Add(this.txtTitluPoveste);
            this.Controls.Add(this.lblStatusuriTitlu);
            this.Controls.Add(this.lstStatusuri);
            this.Controls.Add(this.btnStatusSus);
            this.Controls.Add(this.btnStatusJos);
            this.Controls.Add(this.txtStatusNume);
            this.Controls.Add(this.btnAdaugaStatus);
            this.Controls.Add(this.btnStergeStatus);
            this.Controls.Add(this.lblStructuraTitlu);
            this.Controls.Add(this.treeViewStructura);
            this.Controls.Add(this.btnBlocSus);
            this.Controls.Add(this.btnBlocJos);
            this.Controls.Add(this.btnAdaugaZi);
            this.Controls.Add(this.btnAdaugaBloc);
            this.Controls.Add(this.panelEditareBloc);
            this.Name = "FormEditor";
            this.Text = "Advanced Story Layout Editor - Live Engine";
            this.panelEditareBloc.ResumeLayout(false);
            this.panelEditareBloc.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}